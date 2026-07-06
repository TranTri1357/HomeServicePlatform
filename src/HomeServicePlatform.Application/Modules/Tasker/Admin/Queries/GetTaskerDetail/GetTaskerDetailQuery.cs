using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetTaskerDetail
{
    public record GetTaskerDetailQuery(long TaskerId) : IRequest<ApiResponse<TaskerDetailDto>>;
}
