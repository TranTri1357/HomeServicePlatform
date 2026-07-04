using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerQuickInfo
{
    public class GetTaskerQuickInfoQuery : IRequest<ApiResponse<TaskerQuickInfoDto>>
    {
        public long TaskerId { get; set; }
        public long ServiceId { get; set; }
    }
}
