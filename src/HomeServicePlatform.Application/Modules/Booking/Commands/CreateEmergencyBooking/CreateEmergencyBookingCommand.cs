using System.Collections.Generic;
using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Emergency;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking
{
    /// <summary>
    /// Khách gọi thợ khẩn cấp theo kiểu BROADCAST: chọn 1 dịch vụ + vị trí, hệ thống tạo 1 đơn
    /// treo mở (chưa gán thợ) rồi bắn yêu cầu tới TẤT CẢ thợ đang rảnh trong bán kính. Ai bấm nhận
    /// trước thì đơn thuộc về người đó, các thợ còn lại không nhận được nữa. Giá chốt theo thợ nhận.
    /// CustomerId lấy từ Token. Thanh toán tiền mặt sau khi hoàn thành.
    /// </summary>
    public record CreateEmergencyBookingCommand(
        [property: JsonIgnore] long CustomerId,
        long ServiceId,
        double Latitude,
        double Longitude,
        string FullName,
        string Phone,
        string AddressLine,
        string? ProvinceCode,
        string? DistrictCode,
        string? WardCode,
        string? Note
    ) : IRequest<ApiResponse<CreateEmergencyBookingResponse>>;

    /// <summary>
    /// Dữ liệu để controller đẩy SignalR "ReceiveEmergencyRequest" tới từng thợ (mỗi thợ giá riêng)
    /// và trả về khách để hiển thị màn chờ. <see cref="Taskers"/> có thể rỗng nếu bán kính này chưa
    /// có thợ nào — khi đó frontend tự nới bán kính (re-broadcast).
    /// </summary>
    public record CreateEmergencyBookingResponse(
        long BookingId,
        string ServiceName,
        string AddressLine,
        double Latitude,
        double Longitude,
        int ExpiresInSeconds,
        double RadiusKm,
        IReadOnlyList<EmergencyTaskerOffer> Taskers
    );
}
