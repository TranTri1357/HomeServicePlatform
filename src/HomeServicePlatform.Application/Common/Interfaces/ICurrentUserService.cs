using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        // Trả về UserId kiểu long (hoặc string/Guid tùy DB của bạn)
        long? UserId { get; }

        // Trả về Email người dùng
        string? Email { get; }

        // Trả về danh sách các quyền (Roles) của user đang đăng nhập
        List<string> Roles { get; }

        // Kiểm tra xem user đã đăng nhập hay chưa
        bool IsAuthenticated { get; }
    }
}
