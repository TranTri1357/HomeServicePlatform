using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.CreateTimeOff
{
    public class CreateTimeOffCommandValidator : AbstractValidator<CreateTimeOffCommand>
    {
        public CreateTimeOffCommandValidator()
        {
            RuleFor(x => x.StartAt)
                .NotEmpty().WithMessage("Thời gian bắt đầu không được để trống.")
                .GreaterThan(DateTimeOffset.UtcNow).WithMessage("Thời gian bắt đầu phải lớn hơn thời điểm hiện tại.");

            RuleFor(x => x.EndAt)
                .NotEmpty().WithMessage("Thời gian kết thúc không được để trống.")
                .GreaterThan(x => x.StartAt).WithMessage("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

            RuleFor(x => x.Reason)
                .MaximumLength(200).WithMessage("Lý do không được vượt quá 200 ký tự.");
        }
    }
}
