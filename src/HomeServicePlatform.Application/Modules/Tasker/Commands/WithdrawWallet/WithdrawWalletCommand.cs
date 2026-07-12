using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.WithdrawWallet
{
    /// <summary>
    /// Thợ rút tiền khỏi ví thu nhập (demo: trừ thẳng số dư, không qua ngân hàng
    /// thật). TaskerId lấy từ Token, không nhận từ Client. Trả về số dư mới.
    /// </summary>
    public class WithdrawWalletCommand : IRequest<ApiResponse<decimal>>
    {
        [JsonIgnore] // Lấy từ Token, ẩn khỏi body Swagger
        public long TaskerId { get; set; }

        public decimal Amount { get; set; }
    }
}
