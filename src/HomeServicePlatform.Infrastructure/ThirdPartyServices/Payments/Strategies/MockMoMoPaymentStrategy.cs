using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    public class MockMoMoPaymentStrategy : IPaymentStrategy
    {
        public PaymentMethod Method => PaymentMethod.Momo;

        public Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {
            string transactionCode = $"MOMOMOCK{DateTime.UtcNow:yyyyMMddHHmmss}{bookingId}";

            return Task.FromResult(new PaymentStrategyResult(
                IsInstantSuccess: false,
                PaymentUrl: "mock:momo",
                TransactionCode: transactionCode
            ));
        }
    }
}
