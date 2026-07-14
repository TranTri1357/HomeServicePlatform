using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview
{
    /// <summary>
    /// Xem trước kết quả nếu KHÁCH hủy đơn ngay bây giờ (không ghi DB) — để FE hiển thị
    /// "bạn sẽ được hoàn X đ / giữ Y đ" trước khi bấm xác nhận hủy.
    /// </summary>
    public record GetCancellationPreviewQuery(
        long BookingId,
        long CustomerId
    ) : IRequest<ApiResponse<CancellationPreviewDto>>;
}
