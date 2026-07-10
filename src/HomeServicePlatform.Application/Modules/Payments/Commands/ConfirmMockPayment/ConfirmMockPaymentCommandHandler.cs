using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ConfirmMockPayment
{
    public class ConfirmMockPaymentCommandHandler
        : IRequestHandler<ConfirmMockPaymentCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public ConfirmMockPaymentCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(ConfirmMockPaymentCommand request, CancellationToken ct)
        {
            var payment = await _context.Payments
                .Include(p => p.Booking)
                .FirstOrDefaultAsync(p => p.PaymentId == request.PaymentId, ct);

            if (payment == null)
                throw new NotFoundException($"Không tìm thấy giao dịch thanh toán #{request.PaymentId}.");

            // 🔒 Chỉ chủ đơn mới được xác nhận giao dịch của mình
            if (payment.Booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền xác nhận giao dịch của người khác.");

            // Idempotent: đã thanh toán rồi thì trả về thành công luôn, không xử lý lại
            if (payment.Status == (short)PaymentStatus.Paid)
                return ApiResponse<bool>.Success(true, "Giao dịch đã được thanh toán trước đó.");

            var now = System.DateTimeOffset.UtcNow;

            if (request.Success)
            {
                payment.Status = (short)PaymentStatus.Paid;
                payment.PaidAt = now;

                // 🔔 Thanh toán (cổng demo) thành công -> báo cho (các) thợ được chọn: có đơn mới cần xác nhận.
                await NotifyAssignedTaskersAsync(payment.BookingId, ct);
            }
            else
            {
                payment.Status = (short)PaymentStatus.Cancelled;
            }
            payment.UpdatedAt = now;

            await _context.SaveChangesAsync(ct);

            return request.Success
                ? ApiResponse<bool>.Success(true, "Thanh toán (giả lập) thành công.")
                : ApiResponse<bool>.Success(false, "Đã hủy giao dịch thanh toán.");
        }

        // Gửi thông báo "có đơn mới" tới tất cả thợ được khách chọn sẵn trong đơn.
        private async Task NotifyAssignedTaskersAsync(long bookingId, CancellationToken ct)
        {
            var taskerIds = await _context.BookingItems
                .Where(bi => bi.BookingId == bookingId && bi.TaskerId != null)
                .Select(bi => bi.TaskerId!.Value)
                .Distinct()
                .ToListAsync(ct);

            foreach (var taskerId in taskerIds)
            {
                _context.Notifications.Add(Common.Helpers.NotificationBuilder.Build(
                    taskerId,
                    Domain.Modules.Operations.Enum.NotificationType.NewBooking,
                    "Bạn có đơn mới",
                    $"Bạn có đơn đặt lịch mới (BK{bookingId}) đã thanh toán. Hãy vào xác nhận."));
            }
        }
    }
}
