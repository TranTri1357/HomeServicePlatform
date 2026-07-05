using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.DeleteTimeOff
{
    public class DeleteTimeOffCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long UserId { get; set; }
        public long TimeOffId { get; set; }
    }
}
