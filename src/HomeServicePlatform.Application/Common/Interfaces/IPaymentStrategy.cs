using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public record PaymentStrategyResult(bool IsInstantSuccess, string? PaymentUrl, string? TransactionCode);

    public interface IPaymentStrategy
    {
        PaymentMethod Method { get; }
        Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct);
    }
}
