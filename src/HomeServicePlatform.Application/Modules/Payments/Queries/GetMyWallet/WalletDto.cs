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
        short Type,
        decimal Amount,
        decimal BalanceAfter,
        long? BookingId,
        string Description,
        DateTimeOffset CreatedAt
    );
}
