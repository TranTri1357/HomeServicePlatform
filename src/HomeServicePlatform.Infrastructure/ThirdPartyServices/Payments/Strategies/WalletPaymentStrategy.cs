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
    public class WalletPaymentStrategy : IPaymentStrategy
    {
        private readonly IApplicationDbContext _context;
        public WalletPaymentStrategy(IApplicationDbContext context) => _context = context;

        public PaymentMethod Method => PaymentMethod.Wallet;

        public async Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId, ct);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{bookingId} để thực hiện thanh toán.");

            var wallets = await WalletLedger.ResolveAsync(
                _context,
                new[] { booking.CustomerId, SystemAccounts.EscrowUserId },
                ct);

            var customerWallet = wallets[booking.CustomerId];
            var escrowWallet = wallets[SystemAccounts.EscrowUserId];

            var now = DateTimeOffset.UtcNow;

            WalletLedger.Debit(
                customerWallet,
                WalletTransactionType.Payment,
                amount,
                bookingId,
                now,
                insufficientMessage: "Số dư tài khoản ví điện tử nội bộ không đủ để thực hiện thanh toán đơn hàng này.");

            WalletLedger.Credit(
                escrowWallet,
                WalletTransactionType.EscrowIn,
                amount,
                bookingId,
                now,
                note: $"Giữ hộ tiền đơn BK{bookingId} (thanh toán bằng ví)");

            string localTransactionCode = $"SYSWAL{now:yyyyMMddHHmmss}{bookingId}";

            return new PaymentStrategyResult(
                IsInstantSuccess: true,
                PaymentUrl: null,
                TransactionCode: localTransactionCode
            );
        }
    }
}
