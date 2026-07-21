using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.CreateAddress
{
    public class CreateAddressCommand : IRequest<ApiResponse<long>>
    {
        [JsonIgnore]
        public long CustomerId { get; set; }

        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string AddressLine { get; set; } = default!;
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public bool IsDefault { get; set; }
    }
}
