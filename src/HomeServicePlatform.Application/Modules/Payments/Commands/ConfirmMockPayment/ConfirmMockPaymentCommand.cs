using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ConfirmMockPayment
{
    public record ConfirmMockPaymentCommand(
        long PaymentId,
        bool Success
    ) : IRequest<ApiResponse<bool>>
    {
        [JsonIgnore]
        public long CustomerId { get; init; }
    }
}
