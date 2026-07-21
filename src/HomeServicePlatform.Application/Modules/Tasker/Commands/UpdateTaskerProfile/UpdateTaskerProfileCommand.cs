using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerProfile
{
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
