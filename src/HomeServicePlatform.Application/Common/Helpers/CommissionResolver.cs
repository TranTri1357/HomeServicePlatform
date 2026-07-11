using System;
using System.Collections.Generic;
using System.Linq;
using HomeServicePlatform.Domain.Modules.Operations.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Phân giải tỷ lệ hoa hồng (%) áp cho một (dịch vụ, thợ) tại một thời điểm,
    /// từ danh sách cấu hình <see cref="Commission"/> đã nạp sẵn.
    ///
    /// Độ ưu tiên (cụ thể → tổng quát), lấy bản mới nhất còn hiệu lực:
    ///   1) khớp cả ServiceId + TaskerId
    ///   2) theo dịch vụ (ServiceId, TaskerId = null)
    ///   3) theo thợ     (ServiceId = null, TaskerId)
    ///   4) mặc định toàn hệ thống (ServiceId = null, TaskerId = null)
    /// Không có cấu hình nào phù hợp → 0% (thợ nhận trọn giá).
    /// </summary>
    public static class CommissionResolver
    {
        public static decimal ResolveRate(
            IEnumerable<Commission> commissions,
            long serviceId,
            long taskerId,
            DateTimeOffset at)
        {
            var active = commissions.Where(c => IsActive(c, at)).ToList();

            Commission? Best(Func<Commission, bool> predicate) =>
                active.Where(predicate)
                      .OrderByDescending(c => c.EffectiveFrom)
                      .FirstOrDefault();

            var match =
                   Best(c => c.ServiceId == serviceId && c.TaskerId == taskerId)
                ?? Best(c => c.ServiceId == serviceId && c.TaskerId == null)
                ?? Best(c => c.ServiceId == null && c.TaskerId == taskerId)
                ?? Best(c => c.ServiceId == null && c.TaskerId == null);

            return match?.CommissionRate ?? 0m;
        }

        /// <summary>Số tiền thực nhận (net) sau khi trừ hoa hồng, làm tròn về đồng.</summary>
        public static decimal NetOf(decimal gross, decimal ratePercent) =>
            Math.Round(gross - CommissionOf(gross, ratePercent), 0, MidpointRounding.AwayFromZero);

        /// <summary>Số tiền hoa hồng, làm tròn về đồng.</summary>
        public static decimal CommissionOf(decimal gross, decimal ratePercent) =>
            Math.Round(gross * ratePercent / 100m, 0, MidpointRounding.AwayFromZero);

        private static bool IsActive(Commission c, DateTimeOffset at) =>
            c.EffectiveFrom <= at && (c.EffectiveTo == null || c.EffectiveTo > at);
    }
}
