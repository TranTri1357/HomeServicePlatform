namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    /// <summary>Trạng thái của một lệnh hoàn tiền (khớp cột smallint "status" của bảng refunds).</summary>
    public enum RefundStatus : short
    {
        Pending = 0,    // Đã khởi tạo, chờ xử lý (dùng khi hoàn qua cổng thật, chưa có kết quả)
        Completed = 1,  // Đã hoàn tiền thành công (ví nội bộ: thành công ngay)
        Failed = 2,     // Hoàn tiền thất bại
        Rejected = 3    // Bị từ chối (vd Admin không duyệt hoàn)
    }
}
