using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class TaskerLocationSync
    {
        public static async Task SyncFromDefaultAsync(
            IApplicationDbContext context, long userId, Point? defaultGeom, CancellationToken ct)
        {
            if (defaultGeom == null) return;

            var profile = await context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == userId && !t.IsDeleted, ct);
            if (profile == null) return;

            profile.UpdateLocation(defaultGeom);
        }
    }
}
