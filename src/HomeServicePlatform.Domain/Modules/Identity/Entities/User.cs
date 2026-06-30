using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Customer.Entities;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Entities;

namespace HomeServicePlatform.Domain.Modules.Identity.Entities
{
    public class User
    {
        public long UserId { get; set; }
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public short Status { get; set; } = 1;
        public DateTimeOffset? LastLoginAt { get; set; }
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public int RowVersion { get; set; } = 1;
        public bool IsDeleted { get; set; } = false;

        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
        public virtual TaskerProfile? TaskerProfile { get; set; }
        public virtual Wallet? Wallet { get; set; }
    }
}
