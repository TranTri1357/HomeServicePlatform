using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public readonly record struct RefundOutcome(
        bool Executed,
        decimal RefundedToCustomer,
        decimal CompensatedToTasker);

    public static class RefundExecutor
    {
        public static async Task<RefundOutcome> IssueRefundAsync(
            IApplicationDbContext context,
            Booking booking,
            decimal refundAmount,
            decimal penaltyAmount,
            RefundInitiator initiatedBy,
            string? reason,
            DateTimeOffset now,
            CancellationToken ct)
        {
            var alreadyRefunded = await context.Refunds
                .AnyAsync(r => r.BookingId == booking.BookingId, ct);
            if (alreadyRefunded)
                return new RefundOutcome(false, 0m, 0m);

            var paidPayments = await context.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                .ToListAsync(ct);

            var totalPaid = paidPayments.Sum(p => p.Amount);
            if (totalPaid <= 0m || refundAmount <= 0m && penaltyAmount <= 0m)
                return new RefundOutcome(false, 0m, 0m);

            var taskerId = booking.BookingItems
                .Where(bi => bi.TaskerId.HasValue)
                .Select(bi => bi.TaskerId!.Value)
                .FirstOrDefault();

            var refundDue = Math.Round(Math.Min(refundAmount, totalPaid), 2, MidpointRounding.AwayFromZero);

            var penaltyDue = taskerId > 0
                ? Math.Round(Math.Min(penaltyAmount, totalPaid - refundDue), 2, MidpointRounding.AwayFromZero)
                : 0m;

            var platformFee = totalPaid - refundDue - penaltyDue;

            var refundRatio = totalPaid > 0m ? refundDue / totalPaid : 0m;

            foreach (var payment in paidPayments)
            {
                var portion = Math.Round(payment.Amount * refundRatio, 0, MidpointRounding.AwayFromZero);

                context.Refunds.Add(new Refund
                {
                    PaymentId = payment.PaymentId,
                    BookingId = booking.BookingId,
                    Amount = portion,
                    Status = (short)RefundStatus.Completed,
                    InitiatedBy = (short)initiatedBy,
                    RefundMethod = 0,
                    Reason = reason,
                    CreatedAt = now,
                    CompletedAt = now
                });

                payment.Status = (short)PaymentStatus.Refunded;
                payment.UpdatedAt = now;
            }

            await ReleaseHeldFundsAsync(context, booking.BookingId, totalPaid, now, ct);

            var walletOwners = taskerId > 0
                ? new[] { booking.CustomerId, taskerId, SystemAccounts.RevenueUserId }
                : new[] { booking.CustomerId, SystemAccounts.RevenueUserId };

            var wallets = await WalletLedger.ResolveAsync(context, walletOwners, ct);

            if (refundDue > 0m)
            {
                WalletLedger.Credit(
                    wallets[booking.CustomerId],
                    WalletTransactionType.Refund,
                    refundDue,
                    booking.BookingId,
                    now,
                    note: $"Hoàn tiền đơn BK{booking.BookingId}");
            }

            if (penaltyDue > 0m)
            {
                WalletLedger.Credit(
                    wallets[taskerId],
                    WalletTransactionType.Adjustment,
                    penaltyDue,
                    booking.BookingId,
                    now,
                    note: $"Bồi thường hủy đơn BK{booking.BookingId}");
            }

            if (platformFee > 0m)
            {
                WalletLedger.Credit(
                    wallets[SystemAccounts.RevenueUserId],
                    WalletTransactionType.Commission,
                    platformFee,
                    booking.BookingId,
                    now,
                    note: $"Phí hủy đơn BK{booking.BookingId}");
            }

            return new RefundOutcome(true, refundDue, penaltyDue);
        }

        private static async Task ReleaseHeldFundsAsync(
            IApplicationDbContext context,
            long bookingId,
            decimal totalPaid,
            DateTimeOffset now,
            CancellationToken ct)
        {
            var escrowMovements = await context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.ReferenceId == bookingId
                            && (t.Type == (short)WalletTransactionType.EscrowIn
                                || t.Type == (short)WalletTransactionType.EscrowOut))
                .Select(t => new { t.Type, t.Amount })
                .ToListAsync(ct);

            var stillHeld =
                escrowMovements.Where(t => t.Type == (short)WalletTransactionType.EscrowIn).Sum(t => t.Amount)
                - escrowMovements.Where(t => t.Type == (short)WalletTransactionType.EscrowOut).Sum(t => t.Amount);

            var fromEscrow = Math.Min(totalPaid, Math.Max(stillHeld, 0m));
            var shortfall = totalPaid - fromEscrow;

            var systemWallets = await WalletLedger.ResolveAsync(
                context,
                new[] { SystemAccounts.EscrowUserId, SystemAccounts.RevenueUserId },
                ct);

            if (fromEscrow > 0m)
            {
                WalletLedger.Debit(
                    systemWallets[SystemAccounts.EscrowUserId],
                    WalletTransactionType.EscrowOut,
                    fromEscrow,
                    bookingId,
                    now,
                    note: $"Giải phóng tiền giữ hộ đơn BK{bookingId} để hoàn",
                    insufficientMessage: $"Ví ký quỹ không đủ số dư để hoàn tiền đơn BK{bookingId}.");
            }

            if (shortfall > 0m)
            {
                WalletLedger.Debit(
                    systemWallets[SystemAccounts.RevenueUserId],
                    WalletTransactionType.EscrowOut,
                    shortfall,
                    bookingId,
                    now,
                    note: $"Sàn bù hoàn tiền đơn BK{bookingId} (đơn đã tất toán trước đó)",
                    insufficientMessage: $"Ví doanh thu không đủ số dư để bù hoàn tiền đơn BK{bookingId}.");
            }
        }
    }
}
