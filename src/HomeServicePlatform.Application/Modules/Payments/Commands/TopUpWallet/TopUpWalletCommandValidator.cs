using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet
{
    public class TopUpWalletCommandValidator : AbstractValidator<TopUpWalletCommand>
    {
        public TopUpWalletCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền nạp phải lớn hơn 0.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Số tiền nạp tối đa mỗi lần là 50.000.000đ.");
        }
    }
}
