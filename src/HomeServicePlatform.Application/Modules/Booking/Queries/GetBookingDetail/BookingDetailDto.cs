using System;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetBookingDetail
{
    public record BookingDetailDto(
        long BookingId,
        string CustomerName,
        string ContactName,
        string ContactPhone,
        string? TaskerName,
        short Status,
        decimal SubtotalAmount,
        decimal DiscountAmount,
        decimal FinalAmount,
        string? Note,
        DateTimeOffset CreatedAt,
        string FullAddress
    );
}
