using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateCustomerProfile
{
    public class UpdateCustomerProfileCommandValidator : AbstractValidator<UpdateCustomerProfileCommand>
    {
        public UpdateCustomerProfileCommandValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Vui lòng nhập họ tên.")
                .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Vui lòng nhập số điện thoại.")
                .Matches(@"^(03|05|07|08|09)\d{8}$")
                .WithMessage("Số điện thoại không đúng định dạng di động Việt Nam (10 số).");
        }
    }
}
