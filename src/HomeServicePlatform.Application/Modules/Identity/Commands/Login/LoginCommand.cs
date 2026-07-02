using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.Login
{
    public class LoginCommand : IRequest<ApiResponse<AuthResultDto>>
    {
        public string Identifier { get; set; } = default!; // Nhập Email hoặc Số điện thoại
        public string Password { get; set; } = default!;
    }
}
