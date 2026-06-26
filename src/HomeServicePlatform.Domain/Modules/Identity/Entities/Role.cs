using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HomeServicePlatform.Domain.Modules.Identity.Entities
{
    public class Role
    {
        public short RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
}
