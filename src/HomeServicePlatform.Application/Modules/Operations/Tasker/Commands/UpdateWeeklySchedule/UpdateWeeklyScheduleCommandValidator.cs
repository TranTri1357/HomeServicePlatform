using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.UpdateWeeklySchedule
{
    public class UpdateWeeklyScheduleCommandValidator : AbstractValidator<UpdateWeeklyScheduleCommand>
    {
        public UpdateWeeklyScheduleCommandValidator()
        {
            RuleFor(x => x.Schedules)
                .NotEmpty().WithMessage("Danh sách lịch làm việc không được để trống.");

            RuleForEach(x => x.Schedules).ChildRules(schedule =>
            {
                schedule.RuleFor(s => s.DayOfWeek)
                    .InclusiveBetween((short)0, (short)6).WithMessage("Ngày trong tuần phải từ 0 (CN) đến 6 (T7).");

                schedule.RuleFor(s => s.StartTime)
                    .NotNull().WithMessage("Giờ bắt đầu không hợp lệ.");

                schedule.RuleFor(s => s.EndTime)
                    .NotNull().WithMessage("Giờ kết thúc không hợp lệ.");
            });
        }
    }
}
