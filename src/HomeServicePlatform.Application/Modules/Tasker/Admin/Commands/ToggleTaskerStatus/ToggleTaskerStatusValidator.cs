using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.ToggleTaskerStatus
{
    public class ToggleTaskerStatusValidator : AbstractValidator<ToggleTaskerStatusCommand>
    {
        public ToggleTaskerStatusValidator()
        {
            RuleFor(x => x.TaskerId)
                .GreaterThan(0).WithMessage("Mã hồ sơ thợ không hợp lệ.");
        }
    }
}
