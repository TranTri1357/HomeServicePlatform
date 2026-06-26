using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Services.Entities;

namespace HomeServicePlatform.Domain.Modules.Operations.Entities
{
    public class Commission
    {
        public long CommissionId { get; set; }
        public long? ServiceId { get; set; }
        public long? TaskerId { get; set; }
        public decimal CommissionRate { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? EffectiveTo { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual Service? Service { get; set; }
        public virtual TaskerProfile? TaskerProfile { get; set; }
    }
}
