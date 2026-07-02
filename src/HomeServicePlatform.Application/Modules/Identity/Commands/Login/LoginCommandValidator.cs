using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Identifier).NotEmpty().WithMessage("Vui lòng nhập Email hoặc Số điện thoại.");
            RuleFor(x => x.Password).NotEmpty().WithMessage("Mật khẩu không được để trống.");
        }
    }
}
