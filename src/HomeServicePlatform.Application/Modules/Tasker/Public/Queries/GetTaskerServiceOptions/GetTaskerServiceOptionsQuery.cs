using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerServiceOptions
{
    public record GetTaskerServiceOptionsQuery(long TaskerId)
        : IRequest<ApiResponse<List<TaskerServiceOptionDto>>>;
}
