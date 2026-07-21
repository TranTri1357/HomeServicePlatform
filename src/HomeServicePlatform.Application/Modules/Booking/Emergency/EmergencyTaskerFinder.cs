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
    public record EmergencyTaskerOffer(long TaskerId, string FullName, decimal Price, double DistanceKm);

    public static class EmergencyTaskerFinder
    {
        public const double DegreesPerKm = 111.12;

        public static async Task<List<EmergencyTaskerOffer>> FindEligibleAsync(
            IApplicationDbContext context,
            long serviceId,
            double lat,
            double lng,
            double radiusKm,
            CancellationToken ct,
            IReadOnlyCollection<long>? excludeTaskerIds = null)
        {
            var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
            var customerPoint = geometryFactory.CreatePoint(new Coordinate(lng, lat));
            var radiusInDegrees = radiusKm / DegreesPerKm;
            var now = DateTimeOffset.UtcNow;
            var excluded = excludeTaskerIds?.ToArray() ?? Array.Empty<long>();

            var offers = await context.TaskerProfiles
                .AsNoTracking()
                .Where(t =>
                    !t.IsDeleted &&
                    t.CurrentGeom != null &&
                    t.Status == 1 &&
                    !excluded.Contains(t.TaskerProfileId) &&
                    t.TaskerServices.Any(ts => ts.ServiceId == serviceId) &&
                    t.CurrentGeom.Distance(customerPoint) <= radiusInDegrees)
                .Select(t => new EmergencyTaskerOffer(
                    t.TaskerProfileId,
                    t.User.FullName,
                    t.TaskerServicePrices
                        .Where(p => p.ServiceId == serviceId
                                    && p.EffectiveFrom <= now
                                    && (p.EffectiveTo == null || p.EffectiveTo > now))
                        .OrderByDescending(p => p.EffectiveFrom)
                        .Select(p => p.Price)
                        .FirstOrDefault(),
                    Math.Round(t.CurrentGeom!.Distance(customerPoint) * DegreesPerKm, 1)))
                .ToListAsync(ct);

            return offers.Where(o => o.Price > 0).OrderBy(o => o.DistanceKm).ToList();
        }
    }
}
