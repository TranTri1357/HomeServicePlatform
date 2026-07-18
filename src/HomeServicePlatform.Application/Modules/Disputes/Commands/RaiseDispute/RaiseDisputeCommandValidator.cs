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

            // Cột reason là NOT NULL và không giới hạn độ dài ở DB — chặn ở tầng ứng dụng
            // để tránh nội dung rỗng hoặc quá dài.
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Lý do khiếu nại không được để trống.")
                .MaximumLength(1000).WithMessage("Lý do khiếu nại không được vượt quá 1000 ký tự.");
        }
    }
}
