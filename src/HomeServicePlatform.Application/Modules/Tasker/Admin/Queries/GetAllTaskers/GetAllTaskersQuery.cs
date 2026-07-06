using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetAllTaskers
{
    public record GetAllTaskersQuery(string? SearchTerm, short? Status, int PageIndex = 1, int PageSize = 10)
        : IRequest<ApiResponse<PagedResult<TaskerDto>>>;
}
