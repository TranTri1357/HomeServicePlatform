using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.RejectTasker
{
    public class RejectTaskerValidator : AbstractValidator<RejectTaskerCommand>
    {
        public RejectTaskerValidator()
        {
            RuleFor(x => x.TaskerId)
                .GreaterThan(0).WithMessage("Mã hồ sơ thợ không hợp lệ.");

            RuleFor(x => x.Reason)
                .MaximumLength(500).WithMessage("Lý do từ chối không được vượt quá 500 ký tự.");
        }
    }
}
