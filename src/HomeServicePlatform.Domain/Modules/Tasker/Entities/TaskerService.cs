using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Services.Entities;

namespace HomeServicePlatform.Domain.Modules.Tasker.Entities
{
    public class TaskerService
    {
        public long TaskerId { get; set; }
        public virtual TaskerProfile TaskerProfile { get; set; } = null!;

        public long ServiceId { get; set; }
        public virtual Service Service { get; set; } = null!;
    }
}
