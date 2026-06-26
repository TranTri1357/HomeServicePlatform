using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Services.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? IconUrl { get; set; }
        public bool? IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}
