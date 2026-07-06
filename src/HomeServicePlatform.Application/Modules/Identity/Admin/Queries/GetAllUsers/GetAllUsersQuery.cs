using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetAllUsers
{
    public record GetAllUsersQuery(string? SearchTerm, int PageIndex = 1, int PageSize = 10) 
        : IRequest<ApiResponse<PagedResult<UserDto>>>;
}
