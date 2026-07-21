using System;
using System.Collections.Generic;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class DefaultWorkingSchedule
    {
        public static readonly TimeOnly StartTime = new(8, 0);

        public static readonly TimeOnly EndTime = new(18, 0);

        public static IEnumerable<TaskerSchedule> For(long taskerId)
        {
            for (short day = 1; day <= 6; day++)
            {
                yield return new TaskerSchedule
                {
                    TaskerId = taskerId,
                    DayOfWeek = day,
                    StartTime = StartTime,
                    EndTime = EndTime
                };
            }
        }
    }
}
