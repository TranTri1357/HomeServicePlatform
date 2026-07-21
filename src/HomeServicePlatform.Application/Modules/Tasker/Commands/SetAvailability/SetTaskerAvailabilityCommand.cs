using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.SetAvailability
{
    public class SetTaskerAvailabilityCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long TaskerId { get; set; }

        public bool IsAvailable { get; set; }
    }
}
