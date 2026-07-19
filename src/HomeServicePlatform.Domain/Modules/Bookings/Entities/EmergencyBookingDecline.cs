using System;

namespace HomeServicePlatform.Domain.Modules.Bookings.Entities
{
    /// <summary>
    /// Ghi nhận một thợ đã BỎ QUA một đơn khẩn cấp broadcast — KHÔNG đụng gì tới trạng thái đơn
    /// (đơn vẫn Pending cho các thợ khác nhận).
    ///
    /// Mục đích chính: khi khách nới bán kính (5km → 10km → 15km), tập thợ vòng sau là TẬP CHA của
    /// vòng trước, nên nếu không loại trừ thì thợ đã từ chối sẽ bị dựng dậy lại cho cùng một đơn.
    ///
    /// Chỉ bản ghi <see cref="WasTimeout"/> = false (thợ BẤM từ chối chủ động) mới bị loại khỏi các
    /// vòng sau. Thợ hết 30s không phản hồi vẫn được mời lại — có thể lúc đó họ đang bận tay.
    /// </summary>
    public class EmergencyBookingDecline
    {
        public long EmergencyBookingDeclineId { get; set; }
        public long BookingId { get; set; }
        public long TaskerId { get; set; }

        /// <summary>true = hết 30s không phản hồi; false = thợ chủ động bấm "Từ chối".</summary>
        public bool WasTimeout { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Booking Booking { get; set; } = null!;
    }
}
