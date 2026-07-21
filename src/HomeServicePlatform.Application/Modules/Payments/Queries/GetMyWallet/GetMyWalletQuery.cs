using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetMyWallet
{
    public record GetMyWalletQuery(long CustomerId) : IRequest<ApiResponse<WalletDto>>;
}
