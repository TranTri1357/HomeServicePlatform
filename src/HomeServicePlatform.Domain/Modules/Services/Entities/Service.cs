using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;

namespace HomeServicePlatform.Domain.Modules.Services.Entities
{
    public class Service
    {
        public long ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<TaskerService> TaskerServices { get; set; } = new List<TaskerService>();

        // BỔ SUNG 2 DÒNG NÀY ĐỂ TÍNH TOÁN LƯỢT ĐẶT VÀ GIÁ TIỀN
        public virtual ICollection<BookingItem> BookingItems { get; set; } = new List<BookingItem>();
        public virtual ICollection<TaskerServicePrice> TaskerServicePrices { get; set; } = new List<TaskerServicePrice>();
    }
}
