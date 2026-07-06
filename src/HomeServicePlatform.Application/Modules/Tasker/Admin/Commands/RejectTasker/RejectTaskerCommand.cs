using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.RejectTasker
{
    public record RejectTaskerCommand(long TaskerId, string? Reason) : IRequest<ApiResponse<bool>>;
}
