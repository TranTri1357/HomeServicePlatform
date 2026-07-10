using System;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Identity.Entities;

namespace HomeServicePlatform.Domain.Modules.Operations.Entities
{
    /// <summary>
    /// Tin nhắn chat trong ngữ cảnh một đơn đặt lịch (hội thoại giữa Khách và Thợ).
    /// Một "cuộc trò chuyện" = toàn bộ Message có cùng BookingId.
    /// </summary>
    public class Message
    {
        public long MessageId { get; set; }
        public long BookingId { get; set; }
        public long SenderId { get; set; }
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Booking Booking { get; set; } = null!;
        public virtual User Sender { get; set; } = null!;
    }
}
