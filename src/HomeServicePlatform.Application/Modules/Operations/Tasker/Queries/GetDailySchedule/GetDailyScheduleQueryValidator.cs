using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetDailySchedule
{
    public class GetDailyScheduleQueryValidator : AbstractValidator<GetDailyScheduleQuery>
    {
        public GetDailyScheduleQueryValidator()
        {
            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Vui lòng chọn ngày cần xem lịch.");
        }
    }
}
