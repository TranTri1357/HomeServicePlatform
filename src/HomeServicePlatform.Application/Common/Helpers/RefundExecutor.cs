using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>Số tiền thực tế đã di chuyển sau khi thực thi hoàn tiền.</summary>
    public readonly record struct RefundOutcome(
        bool Executed,          // false nếu không có gì để hoàn (chưa thu tiền / đã hoàn trước đó)
        decimal RefundedToCustomer,
        decimal CompensatedToTasker);

    /// <summary>
    /// Thực thi việc hoàn tiền cho một đơn: tạo bản ghi <see cref="Refund"/>, cộng ví khách
    /// (WalletTransaction loại Refund), đánh dấu Payment = Refunded, và đền phí hủy cho thợ
    /// (WalletTransaction loại Adjustment — KHÔNG khấu hoa hồng vì đây là bồi thường).
    ///
    /// Mọi thay đổi được GẮN vào cùng DbContext caller đang dùng và sẽ được lưu chung trong
    /// một SaveChanges/transaction của caller (đảm bảo nguyên tử). Hàm IDEMPOTENT: nếu đơn đã
    /// có Refund thì bỏ qua, tránh hoàn tiền 2 lần khi client bấm hủy nhiều lần.
    /// </summary>
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
            // Idempotent: đã có lệnh hoàn cho đơn này -> không làm lại.
            var alreadyRefunded = await context.Refunds
                .AnyAsync(r => r.BookingId == booking.BookingId, ct);
            if (alreadyRefunded)
                return new RefundOutcome(false, 0m, 0m);

            // Chỉ hoàn phần đã THỰC SỰ thu qua hệ thống (Payment.Status == Paid).
            // Cash / cổng chưa xác nhận không có tiền trong hệ thống nên không hoàn.
            var paidPayments = await context.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                .ToListAsync(ct);

            var totalPaid = paidPayments.Sum(p => p.Amount);
            if (totalPaid <= 0m || refundAmount <= 0m && penaltyAmount <= 0m)
                return new RefundOutcome(false, 0m, 0m);

            // Tỷ lệ hoàn áp đều cho từng khoản đã thu (vd đơn có cả cọc lẫn trả thêm).
            var refundRatio = totalPaid > 0m ? refundAmount / totalPaid : 0m;

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
                    RefundMethod = 0, // ví nội bộ
                    Reason = reason,
                    CreatedAt = now,
                    CompletedAt = now
                });

                // Đơn đã hủy -> khoản thanh toán coi như đã tất toán bằng hoàn tiền.
                payment.Status = (short)PaymentStatus.Refunded;
                payment.UpdatedAt = now;
            }

            // Cộng tiền hoàn vào ví KHÁCH.
            if (refundAmount > 0m)
                await CreditWalletAsync(context, booking.CustomerId,
                    WalletTransactionType.Refund, refundAmount, booking.BookingId, now, ct);

            // Đền phí hủy cho THỢ đảm nhận đơn (nếu có) dạng Adjustment.
            var taskerId = booking.BookingItems
                .Where(bi => bi.TaskerId.HasValue)
                .Select(bi => bi.TaskerId!.Value)
                .FirstOrDefault();

            if (penaltyAmount > 0m && taskerId > 0)
                await CreditWalletAsync(context, taskerId,
                    WalletTransactionType.Adjustment, penaltyAmount, booking.BookingId, now, ct);

            return new RefundOutcome(true, refundAmount, taskerId > 0 ? penaltyAmount : 0m);
        }

        private static async Task CreditWalletAsync(
            IApplicationDbContext context,
            long userId,
            WalletTransactionType type,
            decimal amount,
            long bookingId,
            DateTimeOffset now,
            CancellationToken ct)
        {
            var wallet = await context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId, ct);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = userId, Balance = 0m };
                context.Wallets.Add(wallet);
            }

            var balanceBefore = wallet.Balance;
            wallet.Balance += amount;

            wallet.WalletTransactions.Add(new WalletTransaction
            {
                Type = (short)type,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                ReferenceId = bookingId,
                CreatedAt = now
            });
        }
    }
}
