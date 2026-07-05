using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Tasker.Entities
{
    public class TaskerTimeOff
    {
        public long TimeOffId { get; set; }
        public long TaskerId { get; set; }
        public DateTimeOffset StartAt { get; set; }
        public DateTimeOffset EndAt { get; set; }
        public string? Reason { get; set; }

        public virtual TaskerProfile TaskerProfile { get; set; } = null!;

        public void RequestTimeOff(DateTimeOffset startAt, DateTimeOffset endAt, string? reason)
        {
            if (startAt >= endAt)
            {
                throw new Exception("Thời gian bắt đầu nghỉ phép phải trước thời gian kết thúc.");
            }

            if (startAt < DateTimeOffset.UtcNow)
            {
                throw new Exception("Không thể xin nghỉ phép cho một thời điểm trong quá khứ.");
            }

            StartAt = startAt;
            EndAt = endAt;
            Reason = reason;
        }
    }
}
