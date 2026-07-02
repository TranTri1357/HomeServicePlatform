using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Dtos
{
    public class AuthResultDto
    {
        public long UserId { get; set; }
        public string FullName { get; set; } = default!;
        public List<string> Roles { get; set; } = new();
        public string AccessToken { get; set; } = default!;
        public string RefreshToken { get; set; } = default!;
        public DateTime AccessTokenExpiresAt { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
    }
}
