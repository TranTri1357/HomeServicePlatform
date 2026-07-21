using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Payments.Entities
{
    public class WalletTransaction
    {
        public long TransactionId { get; set; }
        public long WalletId { get; set; }
        public short Type { get; set; }
        public decimal Amount { get; set; }
        public decimal BalanceBefore { get; set; }
        public decimal BalanceAfter { get; set; }
        public long? ReferenceId { get; set; }

        public string? Note { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Wallet Wallet { get; set; } = null!;
    }
}
