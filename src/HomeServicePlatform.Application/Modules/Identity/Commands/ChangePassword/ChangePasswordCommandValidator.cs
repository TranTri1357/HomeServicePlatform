using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {
        public ChangePasswordCommandValidator()
        {
            RuleFor(x => x.OldPassword)
                .NotEmpty().WithMessage("Vui lòng nhập mật khẩu hiện tại.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
                .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự.")
                .NotEqual(x => x.OldPassword).WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.NewPassword).WithMessage("Xác nhận mật khẩu không khớp với mật khẩu mới.");
        }
    }
}
