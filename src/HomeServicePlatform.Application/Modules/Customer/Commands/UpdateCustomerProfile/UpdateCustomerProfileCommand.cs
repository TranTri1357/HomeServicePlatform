using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateCustomerProfile
{
    public class UpdateCustomerProfileCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long CustomerId { get; set; }

        public string FullName { get; set; } = default!;
        public string Phone { get; set; } = default!;
    }
}
