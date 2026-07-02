using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerServicePrice
{
    public record UpdateTaskerServicePriceCommand(
    long TaskerId,
    long ServiceId,
    decimal NewPrice
) : IRequest<ApiResponse<bool>>;
}
