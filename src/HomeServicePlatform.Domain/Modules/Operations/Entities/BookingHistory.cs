using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;

namespace HomeServicePlatform.Domain.Modules.Operations.Entities
{
    public class BookingHistory
    {
        public long HistoryId { get; set; }
        public long BookingId { get; set; }
        public short? OldStatus { get; set; }
        public short? NewStatus { get; set; }
        public long? ChangedBy { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Booking Booking { get; set; } = null!;
        public virtual User? Changer { get; set; }
    }
}
