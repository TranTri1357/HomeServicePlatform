using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    public class CashPaymentStrategy : IPaymentStrategy
    {
        public PaymentMethod Method => PaymentMethod.Cash;

        public Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {

            string cashTransactionCode = $"CASH{DateTime.UtcNow:yyyyMMddHHmmss}{bookingId}";

            return Task.FromResult(new PaymentStrategyResult(
                IsInstantSuccess: false,
                PaymentUrl: null,
                TransactionCode: cashTransactionCode
            ));
        }
    }
}
