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

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Vui lòng nhập số điện thoại nhận tiền.")
                .Matches(@"^0\d{9}$").WithMessage("Số điện thoại phải gồm 10 chữ số và bắt đầu bằng 0.");

            RuleFor(x => x.BankName)
                .NotEmpty().WithMessage("Vui lòng nhập tên ngân hàng thụ hưởng.")
                .MaximumLength(100).WithMessage("Tên ngân hàng tối đa 100 ký tự.");

            RuleFor(x => x.AccountNumber)
                .NotEmpty().WithMessage("Vui lòng nhập số tài khoản thụ hưởng.")
                .Matches(@"^\d{6,20}$").WithMessage("Số tài khoản phải gồm 6-20 chữ số.");
        }
    }
}
