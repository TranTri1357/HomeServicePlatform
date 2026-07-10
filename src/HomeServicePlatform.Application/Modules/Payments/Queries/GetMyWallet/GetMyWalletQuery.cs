using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetMyWallet
{
    /// <summary>Lấy ví + lịch sử giao dịch gần đây của khách hàng đang đăng nhập.</summary>
    public record GetMyWalletQuery(long CustomerId) : IRequest<ApiResponse<WalletDto>>;
}
