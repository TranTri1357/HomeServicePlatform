using HomeServicePlatform.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace HomeServicePlatform.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // Đọc Claim dạng NameIdentifier (đã lưu UserId lúc Login) và ép kiểu sang long
        public long? UserId
        {
            get
            {
                var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
                return long.TryParse(userIdStr, out long userId) ? userId : null;
            }
        }

        public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);

        // Gom tất cả các claim có type là "role" hoặc ClaimTypes.Role vào danh sách
        public List<string> Roles
        {
            get
            {
                return _httpContextAccessor.HttpContext?.User?.Claims
                    .Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList() ?? new List<string>();
            }
        }

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }

}
