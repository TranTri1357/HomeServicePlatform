namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    /// <summary>Người/nguồn khởi tạo lệnh hoàn tiền (khớp cột smallint "initiated_by" của bảng refunds).</summary>
    public enum RefundInitiator : short
    {
        Customer = 0,   // Khách chủ động hủy đơn
        Tasker = 1,     // Thợ hủy/không thực hiện được -> hoàn 100% cho khách
        Admin = 2,      // Admin can thiệp (tranh chấp, cưỡng chế)
        System = 3      // Hệ thống tự hủy (vd đơn quá hạn)
    }
}
