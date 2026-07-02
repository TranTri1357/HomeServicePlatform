using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    public record GetMyBookingsQuery(long CustomerId) : IRequest<ApiResponse<List<MyBookingDto>>>;
}