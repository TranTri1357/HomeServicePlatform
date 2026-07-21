using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclinePendingBooking
{
    public record DeclinePendingBookingCommand(
        long BookingId,
        string DeclineReason,
        long TaskerId = 0
    ) : IRequest<ApiResponse<bool>>;
}
