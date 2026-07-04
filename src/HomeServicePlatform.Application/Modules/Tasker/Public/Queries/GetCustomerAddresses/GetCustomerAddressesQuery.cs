using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetCustomerAddresses
{
    public class GetCustomerAddressesQuery : IRequest<ApiResponse<List<AddressDto>>>
    {
        [JsonIgnore]
        public long CustomerId { get; set; }
    }
}
