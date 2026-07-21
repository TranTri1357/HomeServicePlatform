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

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly RefundPolicyOptions _policy;
        private readonly BookingPolicyOptions _bookingPolicy;

        public CancelBookingCommandHandler(
            IApplicationDbContext context,
            IOptions<RefundPolicyOptions> policy,
            IOptions<BookingPolicyOptions> bookingPolicy)
        {
            _context = context;
            _policy = policy.Value;
            _bookingPolicy = bookingPolicy.Value;
        }

        public async Task<ApiResponse<bool>> Handle(CancelBookingCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{request.BookingId}");

            var isAssigned = booking.BookingItems.Any(bi => bi.TaskerId == request.TaskerId);
            if (!isAssigned)
                throw new ForbiddenException("Bạn không phụ trách đơn này nên không thể hủy.");

            if (booking.Status != BookingStatus.Accepted
                && booking.Status != BookingStatus.OnTheWay
                && booking.Status != BookingStatus.InProgress)
            {
                throw new BadRequestException("Đơn không ở trạng thái cho phép thợ hủy.");
            }

            var totalPaid = await _context.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var now = DateTimeOffset.UtcNow;

            var decision = RefundPolicy.Calculate(
                booking.Status, null, now, RefundInitiator.Tasker,
                totalPaid, booking.FinalAmount, _bookingPolicy.DepositPercent, _policy);

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
                ChangedBy = request.TaskerId,
                CreatedAt = now
            });

            var outcome = await RefundExecutor.IssueRefundAsync(
                _context, booking,
                decision.RefundAmount, 0m,
                RefundInitiator.Tasker,
                $"Thợ hủy đơn: {request.CancelReason}",
                now, ct);

            await PenalizeTaskerAsync(request.TaskerId, ct);

            _context.Notifications.Add(NotificationBuilder.Build(
                booking.CustomerId,
                NotificationType.BookingCancelledByTasker,
                "Đơn bị hủy",
                $"Thợ đã hủy đơn BK{booking.BookingId}. Lý do: {request.CancelReason}"));

            if (outcome.Executed && outcome.RefundedToCustomer > 0)
            {
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.RefundIssued,
                    "Đã hoàn tiền",
                    $"Đơn BK{booking.BookingId} bị thợ hủy. Hệ thống hoàn 100% ({outcome.RefundedToCustomer:#,##0}đ) vào ví của bạn."));
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true,
                outcome.RefundedToCustomer > 0
                    ? $"Đã hủy đơn và hoàn {outcome.RefundedToCustomer:#,##0}đ cho khách."
                    : "Đã hủy đơn.");
        }

        private async Task PenalizeTaskerAsync(long taskerId, CancellationToken ct)
        {
            var profile = await _context.TaskerProfiles.FirstOrDefaultAsync(t => t.TaskerProfileId == taskerId, ct);
            if (profile == null) return;

            profile.RecordCancellation();

            if (profile.CancelCount >= _policy.TaskerCancelSuspendThreshold)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == taskerId, ct);
                if (user != null)
                    user.Status = 0;
            }
        }
    }
}
