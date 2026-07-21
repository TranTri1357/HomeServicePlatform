using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.DeleteAddress
{
    public record DeleteAddressCommand(long AddressId, long CustomerId) : IRequest<ApiResponse<bool>>;
}
