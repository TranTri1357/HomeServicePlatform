using System;
using System.Linq;
using System.Linq.Expressions;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// 💰 NGUỒN SỰ THẬT DUY NHẤT cho câu hỏi "giá nào của thợ đang có hiệu lực?".
    ///
    /// Trước đây 10 chỗ trong hệ thống tự viết điều kiện này theo 4 kiểu khác nhau
    /// (chỗ chỉ xét <c>EffectiveTo == null</c>, chỗ xét thêm <c>EffectiveTo > now</c>,
    /// chỉ riêng CreateBooking xét đủ cả <c>EffectiveFrom</c> và sắp xếp). Hôm nay chưa
    /// lệch kết quả vì đường GHI không bao giờ tạo giá hẹn trước cho tương lai
    /// (AddTaskerService/UpdateTaskerServicePrice đều gán EffectiveFrom = now), nhưng
    /// khác biệt đó là mìn hẹn giờ: chỉ cần làm tính năng LỊCH SỬ GIÁ cho tử tế
    /// (đóng dòng cũ + chèn dòng mới có EffectiveFrom tương lai) là 9/10 truy vấn sai
    /// cùng lúc, âm thầm, không hề lỗi biên dịch — khách xem một giá, hệ thống tính giá khác.
    ///
    /// Vì vậy: MUỐN ĐỔI ĐỊNH NGHĨA "GIÁ ĐANG HIỆU LỰC" THÌ SỬA DUY NHẤT Ở ĐÂY.
    ///
    /// ⚠️ GIỚI HẠN KỸ THUẬT CỦA EF CORE 8 — đọc trước khi dùng:
    /// EF Core không dịch được lời gọi phương thức tự viết nằm BÊN TRONG lambda của
    /// <c>Select(...)</c>. Nên helper này chỉ dùng được ở truy vấn MỨC GỐC
    /// (<c>_context.TaskerServicePrices...</c>), nơi lời gọi được thực thi ngay lúc dựng
    /// cây biểu thức. Với các chỗ lọc trên navigation collection trong phép chiếu
    /// (ví dụ <c>s.TaskerServicePrices.Where(...)</c> bên trong Select), điều kiện BẮT BUỘC
    /// phải viết thẳng — những chỗ đó đã được ghi chú trỏ ngược về đây để giữ đồng bộ.
    /// </summary>
    public static class TaskerPriceQuery
    {
        /// <summary>
        /// Điều kiện chuẩn: dòng giá đã tới ngày áp dụng và chưa bị đóng hiệu lực.
        /// Tách riêng để vừa dùng lại được, vừa unit-test được mà không cần DB.
        /// </summary>
        public static Expression<Func<TaskerServicePrice, bool>> IsActiveAt(DateTimeOffset now) =>
            p => p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now);

        /// <summary>
        /// Lọc các dòng giá đang hiệu lực tại <paramref name="now"/>, dòng mới áp dụng nhất
        /// đứng trước. Sắp xếp là phần bắt buộc của hợp đồng: nếu có nhiều dòng cùng hiệu lực
        /// thì <c>FirstOrDefault()</c> không kèm thứ tự sẽ trả về kết quả tùy hứng theo
        /// kế hoạch thực thi của Postgres — tức là cùng một đơn có thể ra hai giá khác nhau.
        /// </summary>
        public static IQueryable<TaskerServicePrice> ActiveAt(
            this IQueryable<TaskerServicePrice> source, DateTimeOffset now) =>
            source.Where(IsActiveAt(now))
                  .OrderByDescending(p => p.EffectiveFrom);
    }
}
