using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Booking.Emergency
{
    /// <summary>Một thợ đủ điều kiện nhận đơn khẩn cấp broadcast, kèm giá dịch vụ của chính thợ đó.</summary>
    public record EmergencyTaskerOffer(long TaskerId, string FullName, decimal Price, double DistanceKm);

    /// <summary>
    /// Tìm danh sách thợ đủ điều kiện cho một đơn khẩn cấp broadcast trong một bán kính:
    /// đang RẢNH (Status==1), còn hoạt động, có vị trí, cung cấp dịch vụ, đã cấu hình giá &gt; 0.
    /// Dùng chung cho: tạo (vòng 1), re-broadcast (nới vòng) và báo "đã có thợ khác nhận" cho các thợ thua.
    /// Tái dùng đúng cách tính khoảng cách PostGIS như GetNearbyTaskers (độ ~111.12 km tại xích đạo).
    /// </summary>
    public static class EmergencyTaskerFinder
    {
        public const double DegreesPerKm = 111.12;

        public static async Task<List<EmergencyTaskerOffer>> FindEligibleAsync(
            IApplicationDbContext context,
            long serviceId,
            double lat,
            double lng,
            double radiusKm,
            CancellationToken ct)
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326); // WGS84 (PostGIS)
            var customerPoint = geometryFactory.CreatePoint(new Coordinate(lng, lat));
            var radiusInDegrees = radiusKm / DegreesPerKm;
            var now = DateTimeOffset.UtcNow;

            var offers = await context.TaskerProfiles
                .AsNoTracking()
                .Where(t =>
                    !t.IsDeleted &&
                    t.CurrentGeom != null &&
                    t.Status == 1 && // chỉ thợ đang RẢNH mới nhận đơn khẩn cấp
                    t.TaskerServices.Any(ts => ts.ServiceId == serviceId) &&
                    t.CurrentGeom.Distance(customerPoint) <= radiusInDegrees)
                .Select(t => new EmergencyTaskerOffer(
                    t.TaskerProfileId,
                    t.User.FullName,
                    t.TaskerServicePrices
                        .Where(p => p.ServiceId == serviceId && (p.EffectiveTo == null || p.EffectiveTo > now))
                        .Select(p => p.Price)
                        .FirstOrDefault(),
                    Math.Round(t.CurrentGeom!.Distance(customerPoint) * DegreesPerKm, 1)))
                .ToListAsync(ct);

            // Loại thợ chưa cấu hình giá (Price==0) vì không thể lập hoá đơn; gần nhất lên trước.
            return offers.Where(o => o.Price > 0).OrderBy(o => o.DistanceKm).ToList();
        }
    }
}
