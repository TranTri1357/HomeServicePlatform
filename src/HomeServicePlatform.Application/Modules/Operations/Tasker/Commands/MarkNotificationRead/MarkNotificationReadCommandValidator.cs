using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.MarkNotificationRead
{
    public class MarkNotificationReadCommandValidator : AbstractValidator<MarkNotificationReadCommand>
    {
        public MarkNotificationReadCommandValidator()
        {
            RuleFor(x => x.NotificationId)
                .GreaterThan(0).WithMessage("Mã thông báo không hợp lệ.");
        }
    }
}
