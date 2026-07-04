using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    public class WalletPaymentStrategy : IPaymentStrategy
    {
        private readonly IApplicationDbContext _context;
        public WalletPaymentStrategy(IApplicationDbContext context) => _context = context;

        // Trả về Enum định danh chiến lược thanh toán bằng ví hệ thống (1)
        public PaymentMethod Method => PaymentMethod.Wallet;

        public async Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {
            // 1. Tìm thông tin đơn hàng để xác định chủ đơn (Customer)
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId, ct);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{bookingId} để thực hiện thanh toán.");

            // 2. Tìm tài khoản ví của khách hàng
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == booking.CustomerId, ct);
            if (wallet == null || wallet.Balance < amount)
            {
                throw new BadRequestException("Số dư tài khoản ví điện tử nội bộ không đủ để thực hiện thanh toán đơn hàng này.");
            }

            // 3. Thực hiện trừ tiền trực tiếp trên thực thể ví
            wallet.Balance -= amount;
            //wallet.UpdatedAt = DateTimeOffset.UtcNow;

            // Sinh mã giao dịch nội bộ duy nhất
            string localTransactionCode = $"SYSWAL{DateTime.UtcNow:yyyyMMddHHmmss}{bookingId}";

            // Trả về kết quả: Ví hệ thống thành công ngay lập tức, không cần URL chuyển hướng
            return new PaymentStrategyResult(
                IsInstantSuccess: true,
                PaymentUrl: null,
                TransactionCode: localTransactionCode
            );
        }
    }
}