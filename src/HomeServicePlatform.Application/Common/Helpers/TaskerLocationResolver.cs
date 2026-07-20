using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Khu vực hoạt động của thợ, suy ra từ ĐỊA CHỈ MẶC ĐỊNH của thợ trong bảng Addresses
    /// (Addresses.UserId == TaskerProfileId — thợ và khách dùng chung bảng địa chỉ).
    ///
    /// ⚠️ KHÔNG dùng TaskerProfile.CurrentGeom cho việc này: đó là tọa độ GPS trực tiếp,
    /// chỉ có ý nghĩa cho đơn khẩn cấp (thợ đang ở đâu ngay lúc này) và thường null khi
    /// thợ không mở app. Đặt lịch thường cần "thợ ở khu vực nào" — dữ liệu ổn định.
    /// </summary>
    public sealed record TaskerLocation(string? ProvinceCode, string? DistrictCode, double? Lat, double? Lng);

    public static class TaskerLocationResolver
    {
        /// <summary>
        /// Nạp vị trí của một loạt thợ trong MỘT truy vấn, trả về map theo TaskerId.
        /// Tách khỏi câu truy vấn danh sách thợ (thay vì lồng subquery vào Select) để
        /// tránh sinh SQL khó dịch và giữ phân trang đơn giản.
        /// </summary>
        public static async Task<Dictionary<long, TaskerLocation>> LoadAsync(
            IApplicationDbContext context,
            IReadOnlyCollection<long> taskerIds,
            CancellationToken ct)
        {
            if (taskerIds.Count == 0) return new Dictionary<long, TaskerLocation>();

            var rows = await context.Addresses
                .AsNoTracking()
                .Where(a => taskerIds.Contains(a.UserId))
                // Lấy nguyên cột Geom rồi đọc X/Y trong bộ nhớ, KHÔNG gọi a.Geom.Y trong biểu thức
                // truy vấn: cột nullable sẽ buộc EF sinh CASE WHEN ... ST_Y(...), thứ dễ vỡ khi
                // đổi provider. Điểm đến chỉ vài bản ghi nên tải nguyên Point không tốn kém.
                .Select(a => new
                {
                    a.UserId,
                    a.AddressId,
                    a.ProvinceCode,
                    a.DistrictCode,
                    a.IsDefault,
                    a.Geom
                })
                .ToListAsync(ct);

            // Thợ có thể có nhiều địa chỉ: ưu tiên cái đánh dấu mặc định, không có thì lấy
            // cái tạo sớm nhất — miễn là TẤT ĐỊNH, để danh sách không nhảy giữa các lần gọi.
            return rows
                .GroupBy(r => r.UserId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var a = g.OrderByDescending(x => x.IsDefault == true)
                                 .ThenBy(x => x.AddressId)
                                 .First();
                        return new TaskerLocation(
                            a.ProvinceCode, a.DistrictCode, a.Geom?.Y, a.Geom?.X);
                    });
        }

        /// <summary>
        /// Khoảng cách đường chim bay (km, làm tròn 1 số lẻ) giữa hai tọa độ.
        /// Null nếu thiếu bất kỳ đầu nào — gọi được thẳng với dữ liệu nullable.
        /// </summary>
        public static double? DistanceKm(double? lat1, double? lng1, double? lat2, double? lng2)
        {
            if (lat1 is null || lng1 is null || lat2 is null || lng2 is null) return null;

            const double earthRadiusKm = 6371.0;
            double ToRad(double deg) => deg * Math.PI / 180.0;

            var dLat = ToRad(lat2.Value - lat1.Value);
            var dLng = ToRad(lng2.Value - lng1.Value);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                    + Math.Cos(ToRad(lat1.Value)) * Math.Cos(ToRad(lat2.Value))
                      * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

            return Math.Round(earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a)), 1);
        }
    }
}
