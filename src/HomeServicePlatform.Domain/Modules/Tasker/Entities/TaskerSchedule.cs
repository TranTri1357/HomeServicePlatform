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
    }
}
