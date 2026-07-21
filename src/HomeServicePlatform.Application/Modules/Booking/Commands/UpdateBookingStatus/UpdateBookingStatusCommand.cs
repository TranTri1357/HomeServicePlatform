using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.UpdateBookingStatus
{
    public record UpdateBookingStatusCommand(
        long BookingId,
        short NewStatus,
        long ChangedBy,
        int CurrentRowVersion
    ) : IRequest<ApiResponse<bool>>;
}
