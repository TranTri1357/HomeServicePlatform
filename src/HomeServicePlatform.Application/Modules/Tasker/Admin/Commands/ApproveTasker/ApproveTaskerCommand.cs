using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.ApproveTasker
{
    public record ApproveTaskerCommand(long TaskerId) : IRequest<ApiResponse<bool>>;
}
