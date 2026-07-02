using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Queries.GetCustomerProfile
{
    public record GetCustomerProfileQuery(long CustomerId) : IRequest<ApiResponse<CustomerProfileDto>>;
}
