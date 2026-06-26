using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Payments.Entities
{
    public class Refund
    {
        public long RefundId { get; set; }
        public long PaymentId { get; set; }
        public decimal Amount { get; set; }
        public short Status { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Payment Payment { get; set; } = null!;
    }
}
