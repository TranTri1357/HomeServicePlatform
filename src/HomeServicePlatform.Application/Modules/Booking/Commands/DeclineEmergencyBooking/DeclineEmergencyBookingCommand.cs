using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclineEmergencyBooking
{
    public record DeclineEmergencyBookingCommand(long BookingId, long TaskerId, bool WasTimeout)
        : IRequest<ApiResponse<bool>>;
}
