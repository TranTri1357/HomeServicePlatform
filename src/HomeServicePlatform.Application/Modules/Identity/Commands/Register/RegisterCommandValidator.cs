using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống.")
                .MaximumLength(100).WithMessage("Họ và tên không vượt quá 100 ký tự.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Email không đúng định dạng (VD: example@gmail.com).")
                .MaximumLength(150).WithMessage("Email không vượt quá 150 ký tự.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Số điện thoại không được để trống.")
                .Matches(@"^(0[3|5|7|8|9])+([0-9]{8})$").WithMessage("Số điện thoại không đúng định dạng.");
            
            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Xác nhận mật khẩu không khớp với mật khẩu.");

            RuleFor(x => x.RoleId)
                .Must(roleId => roleId == 2 || roleId == 3)
                .WithMessage("Vai trò đăng ký không hợp lệ.");
        }
    }
}
