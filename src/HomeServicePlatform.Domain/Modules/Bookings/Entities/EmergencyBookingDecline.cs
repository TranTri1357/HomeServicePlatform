using System;

namespace HomeServicePlatform.Domain.Modules.Bookings.Entities
{
    public class EmergencyBookingDecline
    {
        public long EmergencyBookingDeclineId { get; set; }
        public long BookingId { get; set; }
        public long TaskerId { get; set; }

        public bool WasTimeout { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Booking Booking { get; set; } = null!;
    }
}
