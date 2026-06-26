using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Identity.Entities
{
    public class Token
    {
        public long TokenId { get; set; }
        public long UserId { get; set; }
        public string TokenString { get; set; } = null!;
        public DateTimeOffset ExpiredAt { get; set; }
        public short Type { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public virtual User User { get; set; } = null!;
    }
}
