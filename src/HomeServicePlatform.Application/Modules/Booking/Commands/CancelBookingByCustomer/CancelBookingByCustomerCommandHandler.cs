using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBookingByCustomer
{
    public class CancelBookingByCustomerCommandHandler
        : IRequestHandler<CancelBookingByCustomerCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly RefundPolicyOptions _policy;
        private readonly BookingPolicyOptions _bookingPolicy;

        public CancelBookingByCustomerCommandHandler(
            IApplicationDbContext context,
            IOptions<RefundPolicyOptions> policy,
            IOptions<BookingPolicyOptions> bookingPolicy)
        {
            _context = context;
            _policy = policy.Value;
            _bookingPolicy = bookingPolicy.Value;
        }

        public async Task<ApiResponse<bool>> Handle(CancelBookingByCustomerCommand request, CancellationToken ct)
        {
            // 1. Lấy đơn hàng kèm các hạng mục (cần TaskerId + giờ hẹn để áp chính sách).
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{request.BookingId}");

            // 2. 🔒 Chỉ chủ đơn mới được hủy đơn của chính mình.
            if (booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền hủy đơn đặt lịch này.");

            // 3. Tính số tiền đã thu qua hệ thống + giờ hẹn sớm nhất, rồi áp chính sách hoàn tiền.
            var totalPaid = await _context.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var scheduledAt = booking.BookingItems.Count > 0
                ? booking.BookingItems.Min(bi => bi.StartAt)
                : (DateTimeOffset?)null;

            var now = DateTimeOffset.UtcNow;
            var decision = RefundPolicy.Calculate(
                booking.Status, scheduledAt, now, RefundInitiator.Customer,
                totalPaid, booking.FinalAmount, _bookingPolicy.DepositPercent, _policy);

            if (!decision.CanCancel)
                throw new BadRequestException(decision.Reason);

            // 4. Cập nhật trạng thái đơn + hạng mục + ghi Audit Trail.
            short oldStatus = (short)booking.Status;
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = now;

            foreach (var item in booking.BookingItems)
            {
                item.Status = (short)BookingStatus.Cancelled;
                item.CancelRejectReason = request.CancelReason;
                item.UpdatedAt = now;
            }

            _context.BookingHistories.Add(new BookingHistory
            {
                BookingId = booking.BookingId,
                OldStatus = oldStatus,
                NewStatus = (short)BookingStatus.Cancelled,
                ChangedBy = request.CustomerId,
                CreatedAt = now
            });

            // 5. 💸 Thực thi hoàn tiền theo chính sách (idempotent, cộng ví khách + đền thợ).
            var outcome = await RefundExecutor.IssueRefundAsync(
                _context, booking,
                decision.RefundAmount, decision.PenaltyAmount,
                RefundInitiator.Customer,
                $"Khách hủy đơn: {request.CancelReason}",
                now, ct);

            // 6. 🔔 Thông báo cho (các) thợ đã được gán vào đơn.
            var taskerIds = booking.BookingItems
                .Where(bi => bi.TaskerId.HasValue)
                .Select(bi => bi.TaskerId!.Value)
                .Distinct()
                .ToList();
            foreach (var taskerId in taskerIds)
            {
                _context.Notifications.Add(NotificationBuilder.Build(
                    taskerId,
                    NotificationType.BookingCancelledByCustomer,
                    "Khách đã hủy đơn",
                    outcome.CompensatedToTasker > 0
                        ? $"Khách đã hủy đơn BK{booking.BookingId}. Bạn được đền {outcome.CompensatedToTasker:#,##0}đ phí hủy."
                        : $"Khách đã hủy đơn BK{booking.BookingId}."));
            }

            // 6b. 🔔 Báo khách số tiền được hoàn (nếu có).
            if (outcome.Executed && outcome.RefundedToCustomer > 0)
            {
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.RefundIssued,
                    "Đã hoàn tiền",
                    $"Đơn BK{booking.BookingId} đã hủy. Hệ thống hoàn {outcome.RefundedToCustomer:#,##0}đ vào ví của bạn."));
            }

            // 7. Lưu toàn bộ trong một Transaction.
            await _context.SaveChangesAsync(ct);

            var message = outcome.RefundedToCustomer > 0
                ? $"Hủy đơn thành công. Đã hoàn {outcome.RefundedToCustomer:#,##0}đ vào ví ({decision.Reason})"
                : $"Hủy đơn thành công. {decision.Reason}";

            return ApiResponse<bool>.Success(true, message);
        }
    }
}
