using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public sealed record TaskerLocation(string? ProvinceCode, string? DistrictCode, double? Lat, double? Lng);

    public static class TaskerLocationResolver
    {
        public static async Task<Dictionary<long, TaskerLocation>> LoadAsync(
            IApplicationDbContext context,
            IReadOnlyCollection<long> taskerIds,
            CancellationToken ct)
        {
            if (taskerIds.Count == 0) return new Dictionary<long, TaskerLocation>();

            var rows = await context.Addresses
                .AsNoTracking()
                .Where(a => taskerIds.Contains(a.UserId))
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
