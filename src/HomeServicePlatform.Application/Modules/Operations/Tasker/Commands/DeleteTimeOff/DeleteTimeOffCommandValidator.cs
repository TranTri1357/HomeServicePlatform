using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.DeleteTimeOff
{
    public class DeleteTimeOffCommandValidator : AbstractValidator<DeleteTimeOffCommand>
    {
        public DeleteTimeOffCommandValidator()
        {
            RuleFor(x => x.TimeOffId)
                .GreaterThan(0).WithMessage("Mã lịch bận không hợp lệ.");
        }
    }
}
