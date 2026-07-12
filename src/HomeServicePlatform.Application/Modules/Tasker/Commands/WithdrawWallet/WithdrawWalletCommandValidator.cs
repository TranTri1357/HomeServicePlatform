using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.WithdrawWallet
{
    public class WithdrawWalletCommandValidator : AbstractValidator<WithdrawWalletCommand>
    {
        public WithdrawWalletCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(50_000).WithMessage("Số tiền rút tối thiểu mỗi lần là 50.000đ.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Số tiền rút tối đa mỗi lần là 50.000.000đ.");
        }
    }
}
