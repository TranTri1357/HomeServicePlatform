using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerDetail
{
    public class GetTaskerDetailQueryValidator : AbstractValidator<GetTaskerDetailQuery>
    {
        public GetTaskerDetailQueryValidator()
        {
            RuleFor(x => x.TaskerId)
                .GreaterThan(0).WithMessage("Mã thợ không hợp lệ.");
        }
    }
}
