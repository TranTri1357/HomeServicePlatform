using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking
{
    public record CancelBookingCommand(
        long BookingId,
        string CancelReason,
        long TaskerId = 0
    ) : IRequest<ApiResponse<bool>>;
}
