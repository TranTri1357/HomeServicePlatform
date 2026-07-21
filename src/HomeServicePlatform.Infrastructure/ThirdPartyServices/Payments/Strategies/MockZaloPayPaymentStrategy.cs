using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    public class MockZaloPayPaymentStrategy : IPaymentStrategy
    {
        public PaymentMethod Method => PaymentMethod.ZaloPay;

        public Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {
            string transactionCode = $"ZALOMOCK{DateTime.UtcNow:yyyyMMddHHmmss}{bookingId}";

            return Task.FromResult(new PaymentStrategyResult(
                IsInstantSuccess: false,
                PaymentUrl: "mock:zalopay",
                TransactionCode: transactionCode
            ));
        }
    }
}
