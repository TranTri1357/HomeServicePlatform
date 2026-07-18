using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.WithdrawWallet
{
    /// <summary>
    /// Thợ rút tiền khỏi ví thu nhập. Đây là ĐẦU RA của dòng tiền: tiền rời khỏi hệ thống nên
    /// chỉ có một vế ghi nợ ví thợ.
    ///
    /// Demo: không có lệnh chi thật sang ngân hàng (môi trường sandbox không hỗ trợ chi tiền).
    /// Hệ thống ghi nhận đích đến ĐÃ CHE SỐ vào lịch sử giao dịch để đối soát và hiển thị.
    /// TaskerId lấy từ Token, không nhận từ Client. Trả về số dư mới.
    /// </summary>
    public class WithdrawWalletCommand : IRequest<ApiResponse<decimal>>
    {
        [JsonIgnore] // Lấy từ Token, ẩn khỏi body Swagger
        public long TaskerId { get; set; }

        public decimal Amount { get; set; }

        /// <summary>Số điện thoại nhận tiền (ví MoMo / đăng ký ngân hàng). Chỉ lưu 4 số cuối.</summary>
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>Tên ngân hàng thụ hưởng, vd "Vietcombank".</summary>
        public string BankName { get; set; } = string.Empty;

        /// <summary>Số tài khoản thụ hưởng. Chỉ lưu 4 số cuối.</summary>
        public string AccountNumber { get; set; } = string.Empty;
    }
}
