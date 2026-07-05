using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Dtos
{
    public class DailyScheduleDto
    {
        public DateOnly Date { get; set; }

        public List<TimeSlotDto> TimeSlots { get; set; } = new();

        public List<UpcomingJobDto> UpcomingJobs { get; set; } = new();
    }

    public class TimeSlotDto
    {
        public TimeOnly Time { get; set; }

        // 0: Trống (Xanh lá), 1: Đã đặt (Xanh dương), 2: Xin nghỉ/Không làm việc (Xám)
        public short Status { get; set; }
    }

    public class UpcomingJobDto
    {
        public long BookingItemId { get; set; }
        public string ServiceName { get; set; } = default!;
        public string CustomerName { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }

        // Dùng Enum hoặc Short: Sắp tới, Đang thực hiện, Hoàn thành
        public short JobStatus { get; set; }
    }

    public class DailyScheduleInput
    {
        public short DayOfWeek { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
    }
}
