using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.ChangePassword
{
    public class ChangePasswordCommand : IRequest<ApiResponse<bool>>
    {
        public long UserId { get; set; }
        public string OldPassword { get; set; } = default!;
        public string NewPassword { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
