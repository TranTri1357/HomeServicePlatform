using System.Collections.Generic;
using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Emergency;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking
{
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
