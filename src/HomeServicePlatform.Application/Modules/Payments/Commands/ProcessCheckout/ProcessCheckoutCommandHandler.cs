using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommandHandler : IRequestHandler<ProcessCheckoutCommand, ApiResponse<CheckoutResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEnumerable<IPaymentStrategy> _strategies; // Tự động nạp toàn bộ danh sách cổng thanh toán qua DI

        public ProcessCheckoutCommandHandler(IApplicationDbContext context, IEnumerable<IPaymentStrategy> strategies)
        {
            _context = context;
            _strategies = strategies;
        }

        // 💰 Tỷ lệ đặt cọc (khớp với frontend). Cọc 30%, phần còn lại trả khi hoàn thành.
        private const decimal DepositRate = 0.30m;

        public async Task<ApiResponse<CheckoutResponse>> Handle(ProcessCheckoutCommand request, CancellationToken ct)
        {
            // 1. 🛡️ Nạp đơn và XÁC THỰC QUYỀN + TRẠNG THÁI trước khi cho thanh toán.
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);
            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch #{request.BookingId}.");

            // Chỉ chủ đơn mới được thanh toán đơn của mình (chống thanh toán hộ / dò BookingId).
            if (booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền thanh toán đơn hàng của người khác.");

            // Chỉ thanh toán khi đơn còn chờ (Pending). Đơn đã nhận/hủy/hoàn thành thì không cho.
            if (booking.Status != Domain.Modules.Bookings.Enums.BookingStatus.Pending)
                throw new BadRequestException("Đơn hàng không ở trạng thái chờ thanh toán.");

            // Chống trả tiền 2 lần: đã có giao dịch Paid cho đơn này thì chặn.
            var alreadyPaid = await _context.Payments
                .AnyAsync(p => p.BookingId == request.BookingId && p.Status == (short)PaymentStatus.Paid, ct);
            if (alreadyPaid)
                throw new BadRequestException("Đơn hàng này đã được thanh toán trước đó.");

            // 2. 💰 SỐ TIỀN DO SERVER TÍNH từ FinalAmount — KHÔNG tin số client gửi.
            decimal amount = request.IsDeposit
                ? Math.Round(booking.FinalAmount * DepositRate, 0, MidpointRounding.AwayFromZero)
                : booking.FinalAmount;
            if (amount <= 0)
                throw new BadRequestException("Số tiền cần thanh toán của đơn không hợp lệ.");

            // 3. Phân phối chính xác Strategy cần chạy dựa trên thuộc tính Method khách gửi lên
            var strategy = _strategies.FirstOrDefault(s => s.Method == request.Method);
            if (strategy == null)
                throw new BadRequestException("Phương thức thanh toán này hiện chưa được hệ thống hỗ trợ tích hợp.");

            var now = DateTimeOffset.UtcNow;

            // 4. Thực thi logic riêng biệt của cổng thanh toán đó (Trừ tiền ví hoặc sinh link QR)
            var strategyResult = await strategy.ProcessPaymentAsync(request.BookingId, amount, ct);

            // 5. Khởi tạo bản ghi thanh toán đồng bộ đúng cấu trúc Database PostgreSQL của bạn
            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = amount,
                Method = (short)request.Method,
                // Ví nội bộ thành công ngay -> Paid(2); cổng thứ 3 / tiền mặt -> Pending(1) chờ xác nhận.
                Status = (short)(strategyResult.IsInstantSuccess ? PaymentStatus.Paid : PaymentStatus.Pending),
                TransactionCode = strategyResult.TransactionCode,
                PaidAt = strategyResult.IsInstantSuccess ? now : null,
                CreatedAt = now,
                RowVersion = 1 // Phiên bản khởi tạo phục vụ Optimistic Concurrency
            };

            _context.Payments.Add(payment);

            // 4b. 🔔 Đơn đã "chốt" ngay tại bước này -> báo cho (các) thợ được chọn: có đơn mới.
            //     Gồm: ví nội bộ thành công ngay, hoặc tiền mặt (trả khi hoàn thành).
            //     Cổng demo (MoMo/ZaloPay) chưa thành công ở đây; thông báo sẽ bắn khi ConfirmMockPayment.
            if (strategyResult.IsInstantSuccess || (short)request.Method == 2)
            {
                var taskerIds = await _context.BookingItems
                    .Where(bi => bi.BookingId == request.BookingId && bi.TaskerId != null)
                    .Select(bi => bi.TaskerId!.Value)
                    .Distinct()
                    .ToListAsync(ct);

                foreach (var taskerId in taskerIds)
                {
                    _context.Notifications.Add(Common.Helpers.NotificationBuilder.Build(
                        taskerId,
                        Domain.Modules.Operations.Enum.NotificationType.NewBooking,
                        "Bạn có đơn mới",
                        $"Bạn có đơn đặt lịch mới (BK{request.BookingId}) đã thanh toán. Hãy vào xác nhận."));
                }
            }

            // 5. Cập nhật đồng bộ trạng thái đơn hàng (Booking) ngay lập tức nếu thanh toán bằng ví nội bộ thành công
            //if (strategyResult.IsInstantSuccess)
            //{
            //    var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);
            //    if (booking != null)
            //    {
            //        booking.Status = Domain.Modules.Bookings.Enums.BookingStatus.Completed; 
            //        booking.UpdatedAt = now;
            //    }
            //}

            // 6. Lưu thay đổi xuống Database
            await _context.SaveChangesAsync(ct);

            // 7. Trả kết quả về cho Controller để đóng gói thành JSON xuất ra Swagger
            var checkoutResult = new CheckoutResponse(
                payment.PaymentId,
                strategyResult.IsInstantSuccess,
                strategyResult.PaymentUrl
            );

            return ApiResponse<CheckoutResponse>.Success(checkoutResult, "Khởi tạo luồng kết toán hóa đơn thành công.");
        }
    }
}
