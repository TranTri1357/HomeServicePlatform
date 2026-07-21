using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Domain.Modules.Payments.Entities
{
    public class Refund
    {
        public long RefundId { get; set; }
        public long PaymentId { get; set; }

        public long BookingId { get; set; }

        public decimal Amount { get; set; }

        public short Status { get; set; } = (short)RefundStatus.Completed;

        public short InitiatedBy { get; set; } = (short)RefundInitiator.Customer;

        public short RefundMethod { get; set; } = 0;

        public string? Reason { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }

        public virtual Payment Payment { get; set; } = null!;
    }
}
