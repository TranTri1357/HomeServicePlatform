using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetNearbyTaskers
{
    public class GetNearbyTaskersQuery : IRequest<ApiResponse<List<NearbyTaskerDto>>>
    {
        public long ServiceId { get; set; }
        public double CustomerLat { get; set; }
        public double CustomerLng { get; set; }
        public double RadiusKm { get; set; } = 10; // Mặc định 10km
    }
}
