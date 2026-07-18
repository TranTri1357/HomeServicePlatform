using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    /// <summary>
    /// Thanh toán bằng ví nội bộ: tiền KHÔNG biến mất mà chuyển từ ví khách sang ví ký quỹ của
    /// sàn (giữ hộ cho tới khi đơn tất toán). Đây là bút toán chuyển khoản nội bộ hai vế nên
    /// tổng tiền trong hệ thống không đổi.
    ///
    /// Cả hai vế được gắn vào cùng DbContext và lưu chung trong một lần SaveChanges ở
    /// ProcessCheckoutCommandHandler, nên đảm bảo tính nguyên tử.
    /// </summary>
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

            // 2. Nạp ví khách + ví ký quỹ trong MỘT truy vấn
            var wallets = await WalletLedger.ResolveAsync(
                _context,
                new[] { booking.CustomerId, SystemAccounts.EscrowUserId },
                ct);

            var customerWallet = wallets[booking.CustomerId];
            var escrowWallet = wallets[SystemAccounts.EscrowUserId];

            var now = DateTimeOffset.UtcNow;

            // 3. Vế ghi NỢ — trừ ví khách (tự chặn nếu không đủ số dư)
            WalletLedger.Debit(
                customerWallet,
                WalletTransactionType.Payment,
                amount,
                bookingId,
                now,
                insufficientMessage: "Số dư tài khoản ví điện tử nội bộ không đủ để thực hiện thanh toán đơn hàng này.");

            // 4. Vế ghi CÓ — sàn giữ hộ khoản này tới khi đơn hoàn thành hoặc bị hủy
            WalletLedger.Credit(
                escrowWallet,
                WalletTransactionType.EscrowIn,
                amount,
                bookingId,
                now,
                note: $"Giữ hộ tiền đơn BK{bookingId} (thanh toán bằng ví)");

            // Sinh mã giao dịch nội bộ duy nhất
            string localTransactionCode = $"SYSWAL{now:yyyyMMddHHmmss}{bookingId}";

            // Trả về kết quả: Ví hệ thống thành công ngay lập tức, không cần URL chuyển hướng
            return new PaymentStrategyResult(
                IsInstantSuccess: true,
                PaymentUrl: null,
                TransactionCode: localTransactionCode
            );
        }
    }
}
