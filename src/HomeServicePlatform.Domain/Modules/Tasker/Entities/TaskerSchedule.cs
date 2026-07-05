using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Tasker.Entities
{
    public class TaskerSchedule
    {
        public long ScheduleId { get; set; }
        public long TaskerId { get; set; }
        public short DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        public virtual TaskerProfile TaskerProfile { get; set; } = null!;

        public void UpdateWorkingHours(TimeOnly startTime, TimeOnly endTime)
        {
            if (startTime >= endTime)
            {
                throw new Exception("Giờ bắt đầu làm việc phải diễn ra trước giờ kết thúc.");
            }

            var duration = endTime - startTime;
            if (duration.TotalHours < 2)
            {
                throw new Exception("Ca làm việc phải kéo dài tối thiểu 2 giờ.");
            }

            StartTime = startTime;
            EndTime = endTime;
        }
    }
}
