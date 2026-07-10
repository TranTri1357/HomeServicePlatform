using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
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

        public async Task<ApiResponse<CheckoutResponse>> Handle(ProcessCheckoutCommand request, CancellationToken ct)
        {
            // 1. Ràng buộc dữ liệu cơ bản
            if (request.Amount <= 0)
                throw new BadRequestException("Số tiền yêu cầu thanh toán bắt buộc phải lớn hơn 0.");

            // 2. Phân phối chính xác Strategy cần chạy dựa trên thuộc tính Method khách gửi lên
            var strategy = _strategies.FirstOrDefault(s => s.Method == request.Method);
            if (strategy == null)
                throw new BadRequestException("Phương thức thanh toán này hiện chưa được hệ thống hỗ trợ tích hợp.");

            var now = DateTimeOffset.UtcNow;

            // 3. Thực thi logic riêng biệt của cổng thanh toán đó (Trừ tiền ví hoặc sinh link QR)
            var strategyResult = await strategy.ProcessPaymentAsync(request.BookingId, request.Amount, ct);

            // 4. Khởi tạo bản ghi thanh toán đồng bộ đúng cấu trúc Database PostgreSQL của bạn
            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = request.Amount,
                Method = (short)request.Method,
                // Nếu là ví nội bộ hệ thống thì status = 1 (Thành công ngay), các bên thứ 3 hoặc tiền mặt status = 0 (Pending)
                Status = (short)(strategyResult.IsInstantSuccess ? 1 : 0),
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
