using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;

namespace HomeServicePlatform.Domain.Modules.Operations.Entities
{
    public class Dispute
    {
        public long DisputeId { get; set; }
        public long BookingId { get; set; }
        public long RaisedById { get; set; }
        public string Reason { get; set; } = null!;
        public short Status { get; set; } = 0;
        public string? ResolutionNote { get; set; }
        public decimal? RefundAmount { get; set; } = 0;
        public DateTimeOffset? ResolvedAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public int RowVersion { get; set; } = 1;

        public virtual Booking Booking { get; set; } = null!;
        public virtual User RaisedBy { get; set; } = null!;
    }
}
