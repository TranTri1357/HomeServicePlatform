using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclineEmergencyBooking
{
    /// <summary>
    /// Thợ BỎ QUA một đơn khẩn cấp broadcast.
    ///
    /// KHÔNG hủy đơn — đơn vẫn Pending để thợ khác nhận. (Trước đây việc này dùng chung
    /// CancelEmergencyBookingCommand nên vừa sai ngữ nghĩa — một thợ từ chối là chết cả đơn —
    /// vừa luôn ném 403 vì đơn broadcast chưa gán thợ nào để đối chiếu quyền.)
    /// </summary>
    /// <param name="WasTimeout">true = hết 30s không phản hồi; false = thợ chủ động bấm "Từ chối".</param>
    public record DeclineEmergencyBookingCommand(long BookingId, long TaskerId, bool WasTimeout)
        : IRequest<ApiResponse<bool>>;
}
