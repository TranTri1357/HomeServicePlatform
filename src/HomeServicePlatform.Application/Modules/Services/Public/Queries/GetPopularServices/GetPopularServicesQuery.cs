using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetPopularServices
{
    public class GetPopularServicesQuery : IRequest<ApiResponse<List<PopularServiceDto>>>
    {
        public int Limit { get; set; } = 5;
    }
}
