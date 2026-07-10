using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet
{
    /// <summary>
    /// Nạp tiền vào ví (demo: cộng thẳng số dư, không qua cổng thật).
    /// CustomerId lấy từ Token, không nhận từ Client. Trả về số dư mới.
    /// </summary>
    public class TopUpWalletCommand : IRequest<ApiResponse<decimal>>
    {
        [JsonIgnore] // Lấy từ Token, ẩn khỏi body Swagger
        public long CustomerId { get; set; }

        public decimal Amount { get; set; }
    }
}
