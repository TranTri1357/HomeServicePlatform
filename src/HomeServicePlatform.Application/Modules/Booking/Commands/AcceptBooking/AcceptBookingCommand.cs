using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;
using HomeServicePlatform.Application.Common.Responses;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking
{
    public record AcceptBookingCommand(long BookingId, long TaskerId) : IRequest<ApiResponse<bool>>;
}
