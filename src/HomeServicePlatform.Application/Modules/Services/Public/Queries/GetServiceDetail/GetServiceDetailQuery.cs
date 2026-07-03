using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServiceDetail
{
    public class GetServiceDetailQuery : IRequest<ApiResponse<ServiceDetailDto>>
    {
        public long ServiceId { get; set; }
    }
}
