using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
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

            // 🛡️ Chỉ khoản thanh toán qua CỔNG mới được xác nhận bằng endpoint giả lập này.
            //    Nếu bỏ ngỏ, khách có thể gọi confirm lên một khoản Tiền mặt (vốn ở trạng thái
            //    Chờ) để tự đánh dấu đã trả và bơm tiền ảo vào ví ký quỹ — tức là tiêu vào tiền
            //    giữ hộ của các đơn khác.
            if (payment.Method != (short)PaymentMethod.Momo && payment.Method != (short)PaymentMethod.ZaloPay)
                throw new BadRequestException("Chỉ giao dịch qua cổng MoMo/ZaloPay mới dùng được xác nhận giả lập.");

            // Idempotent: đã thanh toán rồi thì trả về thành công luôn, không xử lý lại
            if (payment.Status == (short)PaymentStatus.Paid)
                return ApiResponse<bool>.Success(true, "Giao dịch đã được thanh toán trước đó.");

            // 🛡️ Giao dịch đã hủy/đã hoàn thì không thể "hồi sinh" thành đã trả.
            if (payment.Status != (short)PaymentStatus.Pending)
                throw new BadRequestException("Giao dịch này đã được định đoạt trạng thái, không thể xác nhận lại.");

            var now = System.DateTimeOffset.UtcNow;

            if (request.Success)
            {
                payment.Status = (short)PaymentStatus.Paid;
                payment.PaidAt = now;

                // 💰 ĐẦU VÀO của dòng tiền qua cổng: tiền từ bên ngoài đi thẳng vào ví ký quỹ,
                //    sàn giữ hộ cho tới khi đơn hoàn thành hoặc bị hủy. Không đi qua ví khách
                //    nên chỉ có một vế ghi có (khác với thanh toán bằng ví — chuyển khoản nội bộ).
                var escrowWallet = await WalletLedger.ResolveOneAsync(_context, SystemAccounts.EscrowUserId, ct);

                WalletLedger.Credit(
                    escrowWallet,
                    WalletTransactionType.EscrowIn,
                    payment.Amount,
                    payment.BookingId,
                    now,
                    note: $"Giữ hộ tiền đơn BK{payment.BookingId} ({GatewayName(payment.Method)})");

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

        private static string GatewayName(short method)
            => method == (short)PaymentMethod.ZaloPay ? "ZaloPay" : "MoMo";

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
