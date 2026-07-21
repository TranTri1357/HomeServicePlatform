using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.ResolveDispute
{
    public class ResolveDisputeCommandValidator : AbstractValidator<ResolveDisputeCommand>
    {
        public ResolveDisputeCommandValidator()
        {
            RuleFor(x => x.DisputeId)
                .GreaterThan(0).WithMessage("Mã ca khiếu nại không hợp lệ.");

            RuleFor(x => x.NewStatus)
                .Must(s => s == 1 || s == 2)
                .WithMessage("Phán quyết chỉ nhận giá trị 1 (đồng ý bồi hoàn) hoặc 2 (từ chối).");

            RuleFor(x => x.ResolutionNote)
                .NotEmpty().WithMessage("Vui lòng nhập nội dung ghi chú giải quyết tranh chấp.")
                .MaximumLength(1000).WithMessage("Nội dung phán quyết không được vượt quá 1000 ký tự.");

            RuleFor(x => x.RefundAmount)
                .GreaterThanOrEqualTo(0).WithMessage("Số tiền hoàn không được âm.")
                .LessThanOrEqualTo(500_000_000).WithMessage("Số tiền hoàn vượt quá giới hạn cho phép.")
                .When(x => x.RefundAmount.HasValue);

            RuleFor(x => x.RefundAmount)
                .Must(a => !a.HasValue || a.Value == 0)
                .WithMessage("Ca khiếu nại bị từ chối thì số tiền hoàn phải bằng 0.")
                .When(x => x.NewStatus == 2);
        }
    }
}
