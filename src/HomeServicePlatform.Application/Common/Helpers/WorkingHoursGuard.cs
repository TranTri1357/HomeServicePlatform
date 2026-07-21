using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class WorkingHoursGuard
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        public static async Task EnsureWithinWorkingHoursAsync(
            IApplicationDbContext context,
            long taskerId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct)
        {
            var startVn = startAt.ToOffset(VnOffset);
            var endVn = endAt.ToOffset(VnOffset);

            if (startVn.Date != endVn.Date)
            {
                throw new BadRequestException(
                    "Khung giờ đặt không được kéo dài qua ngày hôm sau. Vui lòng tách thành hai lần đặt.");
            }

            short dayOfWeek = (short)startVn.DayOfWeek;

            var schedule = await context.TaskerSchedules.AsNoTracking()
                .Where(s => s.TaskerId == taskerId && s.DayOfWeek == dayOfWeek)
                .Select(s => new { s.StartTime, s.EndTime })
                .FirstOrDefaultAsync(ct);

            if (schedule == null)
            {
                throw new BadRequestException(
                    $"Thợ không làm việc vào {VietnameseDayName(startVn.DayOfWeek)}. Vui lòng chọn ngày khác.");
            }

            var itemStart = TimeOnly.FromTimeSpan(startVn.TimeOfDay);
            var itemEnd = TimeOnly.FromTimeSpan(endVn.TimeOfDay);

            if (itemStart < schedule.StartTime || itemEnd > schedule.EndTime)
            {
                throw new BadRequestException(
                    $"Giờ hẹn nằm ngoài giờ làm việc của thợ ({schedule.StartTime:HH\\:mm}–{schedule.EndTime:HH\\:mm} " +
                    $"{VietnameseDayName(startVn.DayOfWeek)}). Vui lòng chọn khung giờ khác.");
            }

            bool onTimeOff = await context.TaskerTimeOffs.AsNoTracking()
                .AnyAsync(t => t.TaskerId == taskerId && t.StartAt < endAt && t.EndAt > startAt, ct);

            if (onTimeOff)
            {
                throw new BadRequestException(
                    "Thợ đã báo nghỉ trong khoảng thời gian này. Vui lòng chọn khung giờ khác.");
            }
        }

        private static string VietnameseDayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            _ => "Chủ nhật"
        };
    }
}
