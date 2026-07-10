using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateAddress
{
    /// <summary>Sửa một địa chỉ đã lưu. AddressId từ route, CustomerId từ Token.</summary>
    public class UpdateAddressCommand : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long AddressId { get; set; }

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
