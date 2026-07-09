using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ConfirmMockPayment
{
    /// <summary>
    /// Xác nhận kết quả từ cổng thanh toán GIẢ LẬP (đóng vai IPN/callback).
    /// CustomerId lấy từ Token, không nhận từ Client.
    /// </summary>
    public record ConfirmMockPaymentCommand(
        long PaymentId,
        bool Success
    ) : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore] // Lấy từ Token, ẩn khỏi body Swagger
        public long CustomerId { get; init; }
    }
}
