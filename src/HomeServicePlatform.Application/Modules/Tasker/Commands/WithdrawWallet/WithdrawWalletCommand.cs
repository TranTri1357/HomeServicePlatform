using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.WithdrawWallet
{
    public class WithdrawWalletCommand : IRequest<ApiResponse<decimal>>
    {
        [JsonIgnore]
        public long TaskerId { get; set; }

        public decimal Amount { get; set; }

        public string PhoneNumber { get; set; } = string.Empty;

        public string BankName { get; set; } = string.Empty;

        public string AccountNumber { get; set; } = string.Empty;
    }
}
