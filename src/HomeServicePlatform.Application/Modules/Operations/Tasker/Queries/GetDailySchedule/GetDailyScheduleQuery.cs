using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetDailySchedule
{
    public class GetDailyScheduleQuery : IRequest<ApiResponse<DailyScheduleDto>>
    {
        [JsonIgnore]
        public long UserId { get; set; }

        public DateOnly Date { get; set; }
    }
}
