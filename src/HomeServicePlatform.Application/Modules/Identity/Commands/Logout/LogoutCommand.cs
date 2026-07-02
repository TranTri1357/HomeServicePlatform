using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.Logout
{
    public class LogoutCommand : IRequest<ApiResponse<bool>>
    {
        public string RefreshToken { get; set; } = default!;
    }
}
