using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetAllServices
{
    public record GetAllServicesQuery(string? SearchTerm, int PageIndex = 1, int PageSize = 10)
    : IRequest<ApiResponse<PagedResult<ServiceDto>>>;
}
