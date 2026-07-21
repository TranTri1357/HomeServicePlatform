using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.RebroadcastEmergencyBooking
{
    public record RebroadcastEmergencyBookingCommand(
        [property: JsonIgnore] long CustomerId,
        long BookingId,
        double RadiusKm
    ) : IRequest<ApiResponse<CreateEmergencyBookingResponse>>;
}
