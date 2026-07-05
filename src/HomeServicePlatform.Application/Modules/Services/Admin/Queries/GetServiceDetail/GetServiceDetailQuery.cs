using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetServiceDetail
{
    public record GetServiceDetailQuery(long ServiceId) : IRequest<ApiResponse<ServiceDetailDto>>;
}
