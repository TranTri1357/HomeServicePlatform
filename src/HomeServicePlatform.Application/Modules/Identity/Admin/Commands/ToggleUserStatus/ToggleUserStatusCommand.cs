using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Commands.ToggleUserStatus
{
    public record ToggleUserStatusCommand(long UserId) : IRequest<ApiResponse<bool>>;
}
