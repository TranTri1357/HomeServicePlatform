using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.AddTaskerService
{
    public record AddTaskerServiceCommand(
        long TaskerId,
        long ServiceId,
        decimal Price
    ) : IRequest<ApiResponse<bool>>;
}
