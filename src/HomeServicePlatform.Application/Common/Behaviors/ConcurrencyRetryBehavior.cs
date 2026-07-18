using HomeServicePlatform.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Behaviors
{
    /// <summary>
    /// Chạy lại một lệnh khi nó thua trong cuộc đua ghi đồng thời (<see cref="DbUpdateConcurrencyException"/>).
    ///
    /// VÌ SAO CẦN: bảng wallets dùng cột hệ thống "xmin" của PostgreSQL làm concurrency token
    /// (xem WalletConfiguration) nên hai giao dịch cùng sửa một ví thì người ghi sau bị từ chối
    /// thay vì âm thầm đè mất số dư của người trước. Điều đó chống mất tiền, nhưng nếu không ai
    /// bắt lại thì khách nhận lỗi 500. Ví KÝ QUỸ là điểm nóng nhất vì MỌI đơn đều ghi vào nó,
    /// nên đụng độ là chuyện bình thường chứ không phải ngoại lệ hiếm.
    ///
    /// CÁCH LÀM: chạy lại TOÀN BỘ lệnh, không phải chỉ lặp lại SaveChanges. Bút toán ví là
    /// đọc-sửa-ghi (đọc số dư → cộng/trừ → lưu), nên lần chạy lại bắt buộc phải ĐỌC LẠI số dư
    /// mới. Vì vậy trước mỗi lần thử lại phải xóa sạch ChangeTracker — nếu giữ lại, EF sẽ dùng
    /// đúng bản ghi cũ đã lỗi thời và hỏng thêm lần nữa.
    ///
    /// AN TOÀN VỚI TIỀN: giao dịch thất bại đã rollback nên chưa có gì được ghi xuống DB; chạy
    /// lại từ đầu là ghi lần đầu tiên chứ không phải ghi lần hai. Các luồng tiền còn có sẵn chốt
    /// idempotent riêng (RefundExecutor kiểm tra Refund đã tồn tại, CompleteWork kiểm tra
    /// EscrowOut đã phát sinh) nên kể cả tình huống biên cũng không cộng tiền hai lần.
    ///
    /// Lệnh nào tự bắt DbUpdateConcurrencyException để trả thông báo riêng (UpdateBookingStatus,
    /// ResolveDispute, ProcessPaymentCallback) thì ngoại lệ không bao giờ nổi lên tới đây — hành
    /// vi cũ của chúng giữ nguyên.
    /// </summary>
    public class ConcurrencyRetryBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        /// <summary>Tổng số lần chạy (1 lần đầu + 2 lần thử lại). Đủ cho đụng độ ngẫu nhiên, không đủ để biến thành vòng lặp treo request.</summary>
        private const int MaxAttempts = 3;

        /// <summary>Chờ một nhịp ngắn trước khi thử lại để hai request đang đua không cùng lao vào lại một lúc.</summary>
        private const int BaseDelayMilliseconds = 25;

        private readonly IApplicationDbContext _context;

        public ConcurrencyRetryBehavior(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    return await next();
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxAttempts)
                {
                    // Bỏ hết trạng thái của lần chạy hỏng, để lần sau đọc lại từ DB.
                    _context.ResetTrackedChanges();

                    // Giãn cách tăng dần (25ms, 50ms) + nhiễu ngẫu nhiên để tránh hai request
                    // cùng thức dậy và đụng nhau tiếp.
                    var delay = BaseDelayMilliseconds * attempt + Random.Shared.Next(0, BaseDelayMilliseconds);
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }
    }
}
