using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.SetDefaultAddress
{
    /// <summary>Đặt một địa chỉ làm mặc định. AddressId từ route, CustomerId từ Token.</summary>
    public record SetDefaultAddressCommand(long AddressId, long CustomerId) : IRequest<ApiResponse<bool>>;
}
