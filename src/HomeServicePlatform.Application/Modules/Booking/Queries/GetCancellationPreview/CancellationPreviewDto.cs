namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview
{
    /// <summary>Kết quả xem trước chính sách hủy/hoàn tiền cho một đơn.</summary>
    public record CancellationPreviewDto(
        long BookingId,
        bool CanCancel,        // trạng thái đơn có cho phép khách tự hủy không
        short Status,          // trạng thái đơn hiện tại
        decimal TotalPaid,     // tổng đã thanh toán qua hệ thống
        int RefundPercent,     // % được hoàn TRÊN TIỀN CỌC (không phải trên tổng đã trả)
        decimal RefundAmount,  // tiền hoàn dự kiến cho khách
        decimal PenaltyAmount, // phí hủy giữ lại (đền thợ) — luôn ≤ tiền cọc
        decimal DepositAtRisk, // tiền cọc = phần duy nhất chịu rủi ro khi hủy
        string Reason          // diễn giải để hiển thị cho khách
    );
}
