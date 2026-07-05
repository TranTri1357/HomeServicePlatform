using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings
{
    public record BookingLookupDto(
        long BookingId,
        long CustomerId,
        string CustomerName,
        BookingStatus Status,
        decimal FinalAmount,
        string AddressLine,
        DateTimeOffset CreatedAt,
        int RowVersion
    );

    public record GetPagedBookingsQuery(
        long? CustomerId,
        short? Status,
        DateTimeOffset? FromDate,
        DateTimeOffset? ToDate,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<BookingLookupDto>>>;
}
