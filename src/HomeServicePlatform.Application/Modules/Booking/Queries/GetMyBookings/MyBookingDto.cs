using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    public record MyBookingItemDto(
        long BookingItemId,
        string ServiceName,
        long? TaskerId,
        string? TaskerName,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        short Status,
        bool HasReview
    );

    public record MyBookingDto(
        long BookingId,
        string FullAddress,
        decimal SubtotalAmount,
        decimal? DiscountAmount,
        decimal FinalAmount,
        string? Note,
        DateTimeOffset CreatedAt,
        short Status,
        bool IsPaid,
        bool HasDispute,
        List<MyBookingItemDto> Items
    );
}
