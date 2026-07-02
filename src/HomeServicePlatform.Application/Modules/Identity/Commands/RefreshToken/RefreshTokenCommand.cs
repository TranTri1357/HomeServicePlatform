using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<ApiResponse<AuthResultDto>>
    {
        public string RefreshToken { get; set; } = default!;
    }
}
