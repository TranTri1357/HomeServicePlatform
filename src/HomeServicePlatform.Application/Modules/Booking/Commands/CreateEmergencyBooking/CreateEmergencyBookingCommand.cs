using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking
{
    /// <summary>
    /// Khách gọi thợ khẩn cấp: chọn 1 dịch vụ + 1 thợ đang rảnh gần đó (đã quét trong
    /// bán kính 5km), gửi yêu cầu ngay. CustomerId lấy từ Token. Thanh toán tiền mặt
    /// sau khi hoàn thành nên đơn không cần thanh toán trước.
    /// </summary>
    public record CreateEmergencyBookingCommand(
        [property: JsonIgnore] long CustomerId,
        long ServiceId,
        long TaskerId,
        double Latitude,
        double Longitude,
        string FullName,
        string Phone,
        string AddressLine,
        string? ProvinceCode,
        string? DistrictCode,
        string? WardCode,
        decimal UnitPrice,
        string? Note
    ) : IRequest<ApiResponse<CreateEmergencyBookingResponse>>;

    /// <summary>Dữ liệu để controller đẩy SignalR "ReceiveEmergencyRequest" tới thợ + trả về khách.</summary>
    public record CreateEmergencyBookingResponse(
        long BookingId,
        long TaskerId,
        string ServiceName,
        string AddressLine,
        decimal Amount,
        double DistanceKm,
        int ExpiresInSeconds
    );
}
