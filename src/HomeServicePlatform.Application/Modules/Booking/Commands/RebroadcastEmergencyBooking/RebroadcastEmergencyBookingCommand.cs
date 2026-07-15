using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.RebroadcastEmergencyBooking
{
    /// <summary>
    /// Nới bán kính quét cho một đơn khẩn cấp ĐÃ TẠO nhưng chưa ai nhận (frontend gọi khi hết 1 vòng 30s
    /// mà chưa có thợ nhận: 5km → 10km → 15km). Không tạo đơn mới; chỉ gia hạn cửa sổ phản hồi và trả về
    /// danh sách thợ trong bán kính mới để controller bắn SignalR. CustomerId lấy từ Token (chống spoofing).
    /// </summary>
    public record RebroadcastEmergencyBookingCommand(
        [property: JsonIgnore] long CustomerId,
        long BookingId,
        double RadiusKm
    ) : IRequest<ApiResponse<CreateEmergencyBookingResponse>>;
}
