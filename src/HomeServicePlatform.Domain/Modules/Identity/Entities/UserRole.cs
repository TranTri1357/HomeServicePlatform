using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Identity.Entities
{
    public class UserRole
    {
        public long UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public short RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;
    }
}
