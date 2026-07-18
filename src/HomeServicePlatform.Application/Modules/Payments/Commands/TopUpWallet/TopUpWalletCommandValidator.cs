using FluentValidation;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet
{
    public class TopUpWalletCommandValidator : AbstractValidator<TopUpWalletCommand>
    {
        public TopUpWalletCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền nạp phải lớn hơn 0.")
                .LessThanOrEqualTo(50_000_000).WithMessage("Số tiền nạp tối đa mỗi lần là 50.000.000đ.");

            // 🛡️ Chốt danh sách trắng: chỉ hai cổng giả lập được phép nạp. Nếu bỏ ngỏ, client có
            // thể gửi Method = Wallet/Cash và tự cộng tiền vào ví mà không qua cổng nào cả.
            RuleFor(x => x.Method)
                .Must(m => m == PaymentMethod.Momo || m == PaymentMethod.ZaloPay)
                .WithMessage("Chỉ hỗ trợ nạp ví qua MoMo hoặc ZaloPay.");
        }
    }
}
