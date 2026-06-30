using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Services.Entities;

namespace HomeServicePlatform.Domain.Modules.Bookings.Entities
{
    public class BookingItem
    {
        public long BookingItemId { get; set; }
        public long BookingId { get; set; }
        public long ServiceId { get; set; }
        public long? TaskerId { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public int Quantity { get; set; }
        public int DurationMinutes { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public short Status { get; set; } = 0;
        public string? CancelRejectReason { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public int RowVersion { get; set; } = 1;

        public virtual Booking Booking { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
        public virtual TaskerProfile? TaskerProfile { get; set; }
    }
}
