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
    /// <summary>Số tiền thực tế đã di chuyển sau khi thực thi hoàn tiền.</summary>
    public readonly record struct RefundOutcome(
        bool Executed,          // false nếu không có gì để hoàn (chưa thu tiền / đã hoàn trước đó)
        decimal RefundedToCustomer,
        decimal CompensatedToTasker);

    /// <summary>
    /// Thực thi việc hoàn tiền cho một đơn: tạo bản ghi <see cref="Refund"/>, đánh dấu
    /// Payment = Refunded, rồi GIẢI PHÓNG khoản sàn đang giữ hộ và chia lại cho các bên.
    ///
    /// Bút toán (tổng bằng 0):
    ///     ví ký quỹ    − totalPaid
    ///     ví khách     + refund      (hoàn lại)
    ///     ví thợ       + penalty     (đền phí hủy — KHÔNG khấu hoa hồng vì đây là bồi thường)
    ///     ví doanh thu + phần dôi    (phí hủy sàn giữ lại)
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

            var taskerId = booking.BookingItems
                .Where(bi => bi.TaskerId.HasValue)
                .Select(bi => bi.TaskerId!.Value)
                .FirstOrDefault();

            // Không thể chi ra nhiều hơn số đã thu: chặn trần theo thứ tự ưu tiên
            // (hoàn khách trước, đền thợ sau, phần còn lại là phí hủy của sàn).
            var refundDue = Math.Round(Math.Min(refundAmount, totalPaid), 2, MidpointRounding.AwayFromZero);

            var penaltyDue = taskerId > 0
                ? Math.Round(Math.Min(penaltyAmount, totalPaid - refundDue), 2, MidpointRounding.AwayFromZero)
                : 0m;

            var platformFee = totalPaid - refundDue - penaltyDue;

            // Tỷ lệ hoàn áp đều cho từng khoản đã thu (vd đơn có cả cọc lẫn trả thêm).
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
                    RefundMethod = 0, // ví nội bộ
                    Reason = reason,
                    CreatedAt = now,
                    CompletedAt = now
                });

                // Đơn đã hủy -> khoản thanh toán coi như đã tất toán bằng hoàn tiền.
                payment.Status = (short)PaymentStatus.Refunded;
                payment.UpdatedAt = now;
            }

            // Vế ghi NỢ — lấy tiền ra khỏi nơi đang thực sự giữ nó.
            await ReleaseHeldFundsAsync(context, booking.BookingId, totalPaid, now, ct);

            var walletOwners = taskerId > 0
                ? new[] { booking.CustomerId, taskerId, SystemAccounts.RevenueUserId }
                : new[] { booking.CustomerId, SystemAccounts.RevenueUserId };

            var wallets = await WalletLedger.ResolveAsync(context, walletOwners, ct);

            // Vế ghi CÓ (1) — hoàn lại cho khách.
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

            // Vế ghi CÓ (2) — đền phí hủy cho thợ đảm nhận đơn.
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

            // Vế ghi CÓ (3) — phần dôi là phí hủy sàn giữ lại. Thiếu bước này thì tiền sẽ kẹt
            // vĩnh viễn trong ví ký quỹ của một đơn đã đóng và phá vỡ bất biến đối soát.
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

        /// <summary>
        /// Rút <paramref name="totalPaid"/> ra khỏi nơi đang thực sự giữ nó.
        ///
        /// Bình thường đơn bị hủy trước khi tất toán nên toàn bộ khoản đã thu vẫn nằm trong ví ký
        /// quỹ. Nhưng có trường hợp hoàn tiền SAU khi đơn đã tất toán (vd khiếu nại xử lý muộn):
        /// khi đó ký quỹ đã nhả tiền cho thợ và sàn, nên phần thiếu phải do ví doanh thu gánh —
        /// tuyệt đối không móc vào tiền đang giữ hộ của các đơn khác.
        /// </summary>
        private static async Task ReleaseHeldFundsAsync(
            IApplicationDbContext context,
            long bookingId,
            decimal totalPaid,
            DateTimeOffset now,
            CancellationToken ct)
        {
            // Số còn giữ cho riêng đơn này = đã vào − đã ra (index sẵn có trên reference_id).
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
