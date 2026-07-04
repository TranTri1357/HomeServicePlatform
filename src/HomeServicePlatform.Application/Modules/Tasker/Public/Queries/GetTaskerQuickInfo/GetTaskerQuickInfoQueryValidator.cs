using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerQuickInfo
{
    public class GetTaskerQuickInfoQueryValidator : AbstractValidator<GetTaskerQuickInfoQuery>
    {
        public GetTaskerQuickInfoQueryValidator()
        {
            RuleFor(x => x.TaskerId).GreaterThan(0).WithMessage("Mã thợ không hợp lệ.");
            RuleFor(x => x.ServiceId).GreaterThan(0).WithMessage("Mã dịch vụ không hợp lệ.");
        }
    }
}
