using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    /// <summary>
    /// Cổng ZaloPay GIẢ LẬP (demo, không cần merchant credentials). Xem
    /// <see cref="MockMoMoPaymentStrategy"/> để biết luồng hoạt động.
    /// </summary>
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
