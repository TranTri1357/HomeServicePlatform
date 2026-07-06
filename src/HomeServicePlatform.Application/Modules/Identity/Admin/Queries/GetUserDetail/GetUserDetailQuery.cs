using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetUserDetail
{
    public record GetUserDetailQuery(long UserId) : IRequest<ApiResponse<UserDetailDto>>;
}
