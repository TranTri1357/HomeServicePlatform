using System;
using System.Linq;
using System.Linq.Expressions;
using HomeServicePlatform.Domain.Modules.Operations.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// 🏦 NGUỒN SỰ THẬT DUY NHẤT cho câu hỏi "biểu phí hoa hồng nào đang hiệu lực?".
    /// Song sinh với <see cref="TaskerPriceQuery"/> — xem tài liệu ở đó để hiểu vì sao
    /// tách riêng và vì sao có chỗ phải viết thẳng predicate.
    ///
    /// ⚠️ Khác biệt QUAN TRỌNG so với giá thợ: <see cref="Commission.EffectiveFrom"/>
    /// NHẬN TỪ REQUEST của admin nên có thể nằm ở TƯƠNG LAI. Vì vậy bỏ sót điều kiện
    /// <c>EffectiveFrom &lt;= now</c> ở đây nguy hiểm hơn hẳn: một biểu phí chưa tới ngày
    /// áp dụng sẽ bị tính là "đang chạy", dẫn tới đóng nhầm biểu phí hiện hành và tạo ra
    /// KHOẢNG TRỐNG không có hoa hồng — trong khoảng đó CommissionResolver trả về 0%,
    /// tức là sàn không thu được đồng hoa hồng nào.
    ///
    /// Định nghĩa ở đây PHẢI khớp <see cref="CommissionResolver"/> (nơi tính tiền thật).
    /// </summary>
    public static class CommissionQuery
    {
        /// <summary>Biểu phí đã tới ngày áp dụng và chưa bị đóng hiệu lực tại <paramref name="now"/>.</summary>
        public static Expression<Func<Commission, bool>> IsActiveAt(DateTimeOffset now) =>
            c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now);

        /// <summary>
        /// Biểu phí có khoảng hiệu lực CHỒNG LẤN với khoảng [<paramref name="from"/>, <paramref name="to"/>).
        /// Dùng để chống hai biểu phí cùng áp cho một cặp (dịch vụ, thợ) tại cùng một thời điểm.
        /// Chống chồng lấn phải hỏi "có giao nhau không", KHÔNG phải "có đang chạy lúc này không" —
        /// hỏi sai kiểu thì biểu phí hẹn trước cho tương lai sẽ lọt lưới.
        /// </summary>
        public static Expression<Func<Commission, bool>> OverlapsWith(DateTimeOffset from, DateTimeOffset? to) =>
            c => (c.EffectiveTo == null || c.EffectiveTo > from)
              && (to == null || c.EffectiveFrom < to);

        /// <summary>Lọc các biểu phí đang hiệu lực, bản mới áp dụng nhất đứng trước.</summary>
        public static IQueryable<Commission> ActiveAt(
            this IQueryable<Commission> source, DateTimeOffset now) =>
            source.Where(IsActiveAt(now))
                  .OrderByDescending(c => c.EffectiveFrom);
    }
}
