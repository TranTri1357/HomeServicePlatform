using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

namespace HomeServicePlatform.Domain.Modules.Payments.Entities
{
    public class Refund
    {
        public long RefundId { get; set; }
        public long PaymentId { get; set; }

        // Tham chiếu trực tiếp tới đơn để truy vấn/hiển thị nhanh mà không phải join qua Payment.
        public long BookingId { get; set; }

        public decimal Amount { get; set; }

        // short để khớp cột smallint; giá trị lấy từ enum RefundStatus (0=Pending,1=Completed,...).
        public short Status { get; set; } = (short)RefundStatus.Completed;

        // Ai/nguồn nào khởi tạo hoàn tiền (enum RefundInitiator).
        public short InitiatedBy { get; set; } = (short)RefundInitiator.Customer;

        // 0 = hoàn vào ví nội bộ (mặc định demo); 1 = hoàn về cổng thanh toán
        // (điểm mở rộng cho MoMo/VNPay refund API thật trong tương lai).
        public short RefundMethod { get; set; } = 0;

        public string? Reason { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? CompletedAt { get; set; }

        public virtual Payment Payment { get; set; } = null!;
    }
}
