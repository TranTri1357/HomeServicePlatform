using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetMyWallet
{
    public class WalletDto
    {
        public long WalletId { get; set; }
        public decimal Balance { get; set; }
        public List<WalletTransactionDto> RecentTransactions { get; set; } = new();
    }

    public record WalletTransactionDto(
        long TransactionId,
        short Type,          // 1 TopUp · 2 Payment · 3 Refund · 6 Adjustment
        decimal Amount,
        decimal BalanceAfter,
        long? BookingId,     // đơn liên quan (nếu có) — dùng để hiển thị "đơn nào"
        string Description,  // mô tả rõ lý do + số đơn, hiển thị trực tiếp lên lịch sử
        DateTimeOffset CreatedAt
    );
}
