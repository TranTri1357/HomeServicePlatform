using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelEmergencyBooking
{
    public record CancelEmergencyBookingCommand(long BookingId, long ActorUserId)
        : IRequest<ApiResponse<EmergencyCancelResult>>;

    public record EmergencyCancelResult(
        long CustomerId,
        long TaskerId,
        IReadOnlyList<long> NotifyTaskerIds);
}
