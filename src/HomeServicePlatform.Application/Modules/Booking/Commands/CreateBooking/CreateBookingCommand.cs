using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using HomeServicePlatform.Application.Common.Responses;
namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking
{
    public record CreateBookingCommand(
        long CustomerId,
        string? Note,
        string FullName,
        string Phone,
        string? ProvinceCode,
        string? DistrictCode,
        string? WardCode,
        string AddressLine,
        double? Latitude,
        double? Longitude,
        decimal? DiscountAmount,
        List<BookingItemDto> BookingItems
    ) : IRequest<ApiResponse<CreateBookingResponse>>;
    public record BookingItemDto(
        long ServiceId,
        long? TaskerId,
        DateTime StartAt,
        DateTime EndAt,
        decimal UnitPrice,
        int Quantity
    );
}
