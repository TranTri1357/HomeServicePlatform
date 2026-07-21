using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet
{
    public class TopUpWalletCommand : IRequest<ApiResponse<decimal>>
    {
        [JsonIgnore]
        public long CustomerId { get; set; }

        public decimal Amount { get; set; }

        public PaymentMethod Method { get; set; } = PaymentMethod.Momo;
    }
}
