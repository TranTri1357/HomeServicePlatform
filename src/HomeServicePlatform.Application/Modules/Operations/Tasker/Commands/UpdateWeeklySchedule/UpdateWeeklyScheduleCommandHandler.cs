using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.UpdateWeeklySchedule
{
    public class UpdateWeeklyScheduleCommandHandler : IRequestHandler<UpdateWeeklyScheduleCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateWeeklyScheduleCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateWeeklyScheduleCommand request, CancellationToken ct)
        {
            var taskerProfile = await _context.TaskerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId, ct);
            if (taskerProfile == null) throw new Exception("Bạn chưa hoàn thiện hồ sơ thợ.");

            var actualTaskerId = taskerProfile.TaskerProfileId;
            var existingSchedules = await _context.TaskerSchedules.Where(s => s.TaskerId == actualTaskerId).ToListAsync(ct);
            _context.TaskerSchedules.RemoveRange(existingSchedules);

            foreach (var item in request.Schedules)
            {
                var newSchedule = new TaskerSchedule { TaskerId = actualTaskerId, DayOfWeek = item.DayOfWeek };
                newSchedule.UpdateWorkingHours(item.StartTime, item.EndTime);
                _context.TaskerSchedules.Add(newSchedule);
            }

            await _context.SaveChangesAsync(ct);
            return ApiResponse<bool>.Success(true, "Cài đặt lịch làm việc hàng tuần thành công.");
        }
    }
}
