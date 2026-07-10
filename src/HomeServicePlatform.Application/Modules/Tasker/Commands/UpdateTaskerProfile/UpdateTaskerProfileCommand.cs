using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerProfile
{
    /// <summary>
    /// Thợ tự cập nhật thông tin tài khoản: họ tên, SĐT (bảng Users) và
    /// giới thiệu, số năm kinh nghiệm (bảng TaskerProfiles). UserId lấy từ Token.
    /// </summary>
    public class UpdateTaskerProfileCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long UserId { get; set; }

        public string FullName { get; set; } = default!;
        public string Phone { get; set; } = default!;
        public string? Bio { get; set; }
        public int ExperienceYears { get; set; }
    }
}
