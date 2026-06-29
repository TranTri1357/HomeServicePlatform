using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Queries.GetAllServices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Queries.GetServiceDetail
{
    public record GetServiceDetailQuery(long ServiceId) : IRequest<ApiResponse<ServiceDto>>;
}
