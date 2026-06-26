using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Operations.Entities
{
    public class Notification
    {
        public long NotificationId { get; set; }
        public long UserId { get; set; }
        public short Type { get; set; }
        public string? Payload { get; set; }
        public short Status { get; set; } = 0;
        public int? RetryCount { get; set; } = 0;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual User User { get; set; } = null!;
    }
}
