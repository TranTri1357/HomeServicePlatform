using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.CreatePayment
{
    public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;
        public CreatePaymentCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<long>> Handle(CreatePaymentCommand request, CancellationToken ct)
        {
            // Kiểm tra xem đơn đặt lịch (Booking) có tồn tại không
            var bookingExists = await _context.Bookings.AnyAsync(b => b.BookingId == request.BookingId, ct);
            if (!bookingExists) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            if (request.Amount <= 0) throw new BadRequestException("Số tiền thanh toán phải lớn hơn 0.");

            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = request.Amount,
                Method = request.Method,
                Status = 0, // 0: Pending (Chờ thanh toán)
                TransactionCode = null, // Sẽ cập nhật khi có mã từ cổng thanh toán
                PaidAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                RowVersion = 1 // Giá trị khởi tạo cho concurrency control
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<long>.Success(payment.PaymentId, "Khởi tạo yêu cầu thanh toán thành công.");
        }
    }
}
