using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.StartWorking
{
    public record StartWorkingCommand(long BookingId, long TaskerId) : IRequest<ApiResponse<bool>>;
}
