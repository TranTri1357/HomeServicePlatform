using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateAddress
{
    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator()
        {
            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Vui lòng nhập địa chỉ chi tiết.")
                .MaximumLength(300).WithMessage("Địa chỉ không được vượt quá 300 ký tự.");
        }
    }
}
