using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Commands.DeleteService
{
    public record DeleteServiceCommand(long ServiceId) : IRequest<ApiResponse<bool>>;
}
