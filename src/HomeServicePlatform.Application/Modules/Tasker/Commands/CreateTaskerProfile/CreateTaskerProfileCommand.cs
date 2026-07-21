using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile
{
    public class CreateTaskerProfileCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long UserId { get; set; }
        public string Bio { get; set; } = default!;
        public int ExperienceYears { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public string VerificationImageUrl { get; set; } = default!;
    }
}
