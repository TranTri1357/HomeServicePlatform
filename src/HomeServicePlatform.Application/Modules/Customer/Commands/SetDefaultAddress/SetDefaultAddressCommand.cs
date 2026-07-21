using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.SetDefaultAddress
{
    public record SetDefaultAddressCommand(long AddressId, long CustomerId) : IRequest<ApiResponse<bool>>;
}
