using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;

namespace HomeServicePlatform.Domain.Modules.Payments.Entities
{
    public class Payment
    {
        public long PaymentId { get; set; }
        public long BookingId { get; set; }
        public decimal Amount { get; set; }
        public short Method { get; set; }
        public short Status { get; set; } = 0;
        public string? TransactionCode { get; set; }
        public DateTimeOffset? PaidAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public int RowVersion { get; set; } = 1;

        public virtual Booking Booking { get; set; } = null!;
        public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
    }
}
