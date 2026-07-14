namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview
{
    /// <summary>Kết quả xem trước chính sách hủy/hoàn tiền cho một đơn.</summary>
    public record CancellationPreviewDto(
        long BookingId,
        bool CanCancel,        // trạng thái đơn có cho phép khách tự hủy không
        short Status,          // trạng thái đơn hiện tại
        decimal TotalPaid,     // tổng đã thanh toán qua hệ thống
        int RefundPercent,     // % hoàn theo chính sách
        decimal RefundAmount,  // tiền hoàn dự kiến cho khách
        decimal PenaltyAmount, // phí hủy giữ lại (đền thợ)
        string Reason          // diễn giải để hiển thị cho khách
    );
}
