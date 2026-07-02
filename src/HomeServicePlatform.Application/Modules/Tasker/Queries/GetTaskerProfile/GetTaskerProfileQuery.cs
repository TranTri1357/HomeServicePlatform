using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerProfile
{
    public record GetTaskerProfileQuery(long TaskerProfileId) : IRequest<ApiResponse<TaskerProfileDto>>;
}
