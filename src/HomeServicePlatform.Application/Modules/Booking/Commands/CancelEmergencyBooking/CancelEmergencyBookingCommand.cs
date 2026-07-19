using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelEmergencyBooking
{
    /// <summary>
    /// Hủy một đơn khẩn cấp còn ở trạng thái chờ (Pending): dùng cho cả khách
    /// (hết 30s / chọn thợ khác) lẫn thợ (bấm từ chối). ActorUserId phải là chủ đơn
    /// hoặc thợ được gán. Trả về CustomerId + danh sách thợ cần báo để controller đẩy SignalR.
    /// </summary>
    public record CancelEmergencyBookingCommand(long BookingId, long ActorUserId)
        : IRequest<ApiResponse<EmergencyCancelResult>>;

    /// <param name="TaskerId">Thợ ĐƯỢC GÁN, hoặc 0 với đơn broadcast chưa ai nhận.</param>
    /// <param name="NotifyTaskerIds">
    /// Danh sách thợ cần bắn "ReceiveEmergencyCancelled" để đóng modal đang kêu chuông.
    /// Với đơn broadcast thì TaskerId luôn = 0, nên KHÔNG được dùng nó làm đích gửi.
    /// </param>
    public record EmergencyCancelResult(
        long CustomerId,
        long TaskerId,
        IReadOnlyList<long> NotifyTaskerIds);
}
