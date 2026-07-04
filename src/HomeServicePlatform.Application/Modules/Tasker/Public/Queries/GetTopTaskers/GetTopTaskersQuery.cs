using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTopTaskers
{
    public class GetTopTaskersQuery : IRequest<ApiResponse<List<TopTaskerDto>>>
    {
        public int Limit { get; set; } = 5;
    }
}
