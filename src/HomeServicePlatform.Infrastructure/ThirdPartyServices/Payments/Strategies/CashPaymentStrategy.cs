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

        // Không có thao tác bất đồng bộ nào (tiền mặt không gọi cổng ngoài) nên trả Task
        // hoàn thành sẵn thay vì đánh dấu async — tránh chi phí máy trạng thái thừa.
        public Task<PaymentStrategyResult> ProcessPaymentAsync(long bookingId, decimal amount, CancellationToken ct)
        {
            // Nghiệp vụ tiền mặt (Thợ làm xong mới thu tiền mặt):
            // Giao dịch khởi tạo sẽ ở trạng thái Chờ thanh toán (IsInstantSuccess = false)
            // Cho đến khi thợ xác nhận đã cầm tiền mặt từ khách, Admin/Thợ mới bấm duyệt chuyển status thành Thành công.

            // Sinh mã giao dịch tiền mặt nội bộ để quản lý đối soát
            string cashTransactionCode = $"CASH{DateTime.UtcNow:yyyyMMddHHmmss}{bookingId}";

            return Task.FromResult(new PaymentStrategyResult(
                IsInstantSuccess: false, // 🟢 Bằng false vì tiền mặt chưa được thu ngay lúc đặt lịch
                PaymentUrl: null,        // Tiền mặt không cần link chuyển hướng cổng thanh toán
                TransactionCode: cashTransactionCode
            ));
        }
    }
}
