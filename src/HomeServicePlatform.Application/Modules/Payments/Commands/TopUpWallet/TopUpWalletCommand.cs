using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet
{
    /// <summary>
    /// Nạp tiền vào ví (demo: cộng thẳng số dư, không qua cổng thật).
    ///
    /// Trải nghiệm hai bước nằm ở Frontend: khách chọn cổng, màn QR giả lập (dùng lại đúng
    /// component của luồng thanh toán đơn) hiện lên, bấm "Xác nhận thanh toán" thì FE mới gọi
    /// endpoint này đúng một lần. Backend không giữ phiên chờ nào — đây là ĐẦU VÀO của dòng
    /// tiền nên chỉ có một vế ghi có.
    ///
    /// CustomerId lấy từ Token, không nhận từ Client. Trả về số dư mới.
    /// </summary>
    public class TopUpWalletCommand : IRequest<ApiResponse<decimal>>
    {
        [JsonIgnore] // Lấy từ Token, ẩn khỏi body Swagger
        public long CustomerId { get; set; }

        public decimal Amount { get; set; }

        /// <summary>Cổng nạp giả lập: chỉ chấp nhận MoMo (3) hoặc ZaloPay (4).</summary>
        public PaymentMethod Method { get; set; } = PaymentMethod.Momo;
    }
}
