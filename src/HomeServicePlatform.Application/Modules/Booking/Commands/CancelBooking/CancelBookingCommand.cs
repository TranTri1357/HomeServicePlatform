using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking
{
    /// <summary>
    /// Thợ hủy đơn đã nhận. TaskerId được controller đè từ Token (chống spoofing).
    /// Khách được hoàn 100% và thợ bị ghi nhận 1 lần hủy (hạ độ tin cậy).
    /// </summary>
    public record CancelBookingCommand(
        long BookingId,
        string CancelReason,
        long TaskerId = 0
    ) : IRequest<ApiResponse<bool>>;
}
