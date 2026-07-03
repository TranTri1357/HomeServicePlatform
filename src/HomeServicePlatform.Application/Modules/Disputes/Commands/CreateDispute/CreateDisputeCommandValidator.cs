using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.CreateDispute
{
    public class CreateDisputeCommandValidator : AbstractValidator<CreateDisputeCommand>
    {
        public CreateDisputeCommandValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Vui lòng nhập lý do khiếu nại.")
                .MinimumLength(10).WithMessage("Lý do khiếu nại cần chi tiết hơn (ít nhất 10 ký tự).")
                .MaximumLength(1000).WithMessage("Lý do khiếu nại không được vượt quá 1000 ký tự.");
        }
    }
}
