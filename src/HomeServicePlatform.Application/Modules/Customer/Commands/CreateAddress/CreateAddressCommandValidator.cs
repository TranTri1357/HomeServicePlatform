using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.CreateAddress
{
    public class CreateAddressCommandValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressCommandValidator()
        {
            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Vui lòng nhập địa chỉ chi tiết.")
                .MaximumLength(300).WithMessage("Địa chỉ không được vượt quá 300 ký tự.");
        }
    }
}
