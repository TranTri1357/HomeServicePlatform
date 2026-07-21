using System;
using System.Linq;
using System.Linq.Expressions;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class TaskerPriceQuery
    {
        public static Expression<Func<TaskerServicePrice, bool>> IsActiveAt(DateTimeOffset now) =>
            p => p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now);

        public static IQueryable<TaskerServicePrice> ActiveAt(
            this IQueryable<TaskerServicePrice> source, DateTimeOffset now) =>
            source.Where(IsActiveAt(now))
                  .OrderByDescending(p => p.EffectiveFrom);
    }
}
