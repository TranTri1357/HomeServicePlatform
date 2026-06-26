using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Payments.Entities
{
    public class Wallet
    {
        public long WalletId { get; set; }
        public long UserId { get; set; }
        public decimal Balance { get; set; } = 0;

        public virtual User User { get; set; } = null!;
        public virtual ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
