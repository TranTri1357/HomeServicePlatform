using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.ChangePassword
{
    /// <summary>
    /// Đổi mật khẩu khi đã đăng nhập. UserId LẤY TỪ TOKEN (controller đè), không nhận từ body.
    /// </summary>
    public class ChangePasswordCommand : IRequest<ApiResponse<bool>>
    {
        public long UserId { get; set; }
        public string OldPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
