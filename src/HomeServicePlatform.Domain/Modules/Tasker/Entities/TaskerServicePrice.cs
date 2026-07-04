using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Services.Entities;

namespace HomeServicePlatform.Domain.Modules.Tasker.Entities
{
    public class TaskerServicePrice
    {
        public long TaskerServicePriceId { get; set; }
        public long TaskerId { get; set; }
        public long ServiceId { get; set; }
        public decimal Price { get; set; }
        public DateTimeOffset EffectiveFrom { get; set; }
        public DateTimeOffset? EffectiveTo { get; set; }

        public virtual TaskerProfile TaskerProfile { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;

        public void Expire(DateTimeOffset expirationTime)
        {
            if (EffectiveTo.HasValue && EffectiveTo.Value <= expirationTime)
                throw new InvalidOperationException("Mức giá này đã hết hạn từ trước.");

            EffectiveTo = expirationTime;
        }
    }
}
