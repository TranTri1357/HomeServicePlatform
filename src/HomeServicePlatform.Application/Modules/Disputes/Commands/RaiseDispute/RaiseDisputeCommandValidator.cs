using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.RaiseDispute
{
    public class RaiseDisputeCommandValidator : AbstractValidator<RaiseDisputeCommand>
    {
        public RaiseDisputeCommandValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");

            RuleFor(x => x.RaisedById)
                .GreaterThan(0).WithMessage("Mã người gửi khiếu nại không hợp lệ.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do khiếu nại không được để trống.")
                .MaximumLength(1000).WithMessage("Lý do khiếu nại không được vượt quá 1000 ký tự.");
        }
    }
}
