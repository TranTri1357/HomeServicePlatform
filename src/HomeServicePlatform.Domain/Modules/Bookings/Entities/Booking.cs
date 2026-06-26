using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Entities;

namespace HomeServicePlatform.Domain.Modules.Bookings.Entities
{
    public class Booking
    {
public long BookingId { get; set; }
        public long CustomerId { get; set; }
        public short Status { get; set; } = 0;
        public decimal SubtotalAmount { get; set; }
        public decimal? DiscountAmount { get; set; } = 0;
        public decimal FinalAmount { get; set; }
        public string? Note { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public int RowVersion { get; set; } = 1;

        // Navigation Properties
        public virtual User Customer { get; set; } = null!;
        public virtual BookingAddress? BookingAddress { get; set; }
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public virtual ICollection<BookingHistory> BookingHistories { get; set; } = new List<BookingHistory>();
        public virtual ICollection<Dispute> Disputes { get; set; } = new List<Dispute>();
    }
}
