using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Operations.Entities
{
    public class Review
    {
        public long ReviewId { get; set; }
        public long BookingItemId { get; set; }
        public long TaskerId { get; set; }
        public long CustomerId { get; set; }
        public short Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public virtual BookingItem BookingItem { get; set; } = null!;
        public virtual TaskerProfile TaskerProfile { get; set; } = null!;
        public virtual User Customer { get; set; } = null!;
    }
}
