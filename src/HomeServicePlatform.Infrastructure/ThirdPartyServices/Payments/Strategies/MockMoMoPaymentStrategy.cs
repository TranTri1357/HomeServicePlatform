using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Infrastructure.ThirdPartyServices.Payments.Strategies
{
    /// <summary>
    /// Cổng MoMo GIẢ LẬP (demo, không cần merchant credentials).
    /// Mô phỏng đúng luồng cổng thật: tạo giao dịch ở trạng thái Chờ + trả về
    /// một "PaymentUrl" đánh dấu (mock:momo) để Frontend mở trang cổng giả lập.
    /// Việc xác nhận đã trả tiền diễn ra qua endpoint /api/payments/mock/confirm
    /// (đóng vai IPN/callback của cổng).
    /// </summary>
    public class MockMoMoPaymentStrategy : IPaymentStrategy
    {
        public PaymentMethod Method => PaymentMethod.Momo;

        public Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {
            string transactionCode = $"MOMOMOCK{DateTime.UtcNow:yyyyMMddHHmmss}{bookingId}";

            return Task.FromResult(new PaymentStrategyResult(
                IsInstantSuccess: false, // Chưa trả tiền ngay — chờ khách xác nhận trên trang cổng
                PaymentUrl: "mock:momo", // Marker để Frontend nhận biết mở cổng giả lập
                TransactionCode: transactionCode
            ));
        }
    }
}
