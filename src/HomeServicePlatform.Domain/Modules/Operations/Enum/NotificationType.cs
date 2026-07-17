namespace HomeServicePlatform.Domain.Modules.Operations.Enum
{
    /// <summary>Loại thông báo (khớp cột smallint "type" của bảng notifications).</summary>
    public enum NotificationType : short
    {
        // Gửi cho Thợ
        NewBooking = 1,               // Khách đặt đơn mới cho thợ
        BookingCancelledByCustomer = 2, // Khách hủy đơn
        NewReview = 3,                // Khách đánh giá thợ
        EmergencyBooking = 4,         // Khách gọi thợ khẩn cấp (đơn trực tiếp, cần phản hồi trong 30s)
        ProfileApproved = 5,          // Admin duyệt hồ sơ thợ
        ProfileRejected = 6,          // Admin từ chối hồ sơ thợ (kèm lý do trong body)

        // Gửi cho Khách
        BookingAccepted = 10,         // Thợ nhận đơn
        TaskerOnTheWay = 11,          // Thợ đang đến
        WorkStarted = 12,             // Thợ bắt đầu làm
        WorkCompleted = 13,           // Hoàn thành công việc
        BookingCancelledByTasker = 14, // Thợ hủy đơn
        RefundIssued = 15             // Đã hoàn tiền vào ví khách sau khi hủy đơn
    }
}
