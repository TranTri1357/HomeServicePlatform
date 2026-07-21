using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobsPaged
{
    public record GetTaskerJobsPagedQuery(
        long TaskerId,
        IReadOnlyList<short>? Statuses = null,
        string? SearchTerm = null,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<TaskerJobGroupDto>>>;
}
