using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.DeleteAddress
{
    /// <summary>Xóa một địa chỉ đã lưu. AddressId từ route, CustomerId từ Token.</summary>
    public record DeleteAddressCommand(long AddressId, long CustomerId) : IRequest<ApiResponse<bool>>;
}
