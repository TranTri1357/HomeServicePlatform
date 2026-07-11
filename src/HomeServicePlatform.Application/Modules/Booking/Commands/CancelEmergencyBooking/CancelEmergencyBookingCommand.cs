using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelEmergencyBooking
{
    /// <summary>
    /// Hủy một đơn khẩn cấp còn ở trạng thái chờ (Pending): dùng cho cả khách
    /// (hết 30s / chọn thợ khác) lẫn thợ (bấm từ chối). ActorUserId phải là chủ đơn
    /// hoặc thợ được gán. Trả về CustomerId + TaskerId để controller đẩy SignalR.
    /// </summary>
    public record CancelEmergencyBookingCommand(long BookingId, long ActorUserId)
        : IRequest<ApiResponse<EmergencyCancelResult>>;

    public record EmergencyCancelResult(long CustomerId, long TaskerId);
}
