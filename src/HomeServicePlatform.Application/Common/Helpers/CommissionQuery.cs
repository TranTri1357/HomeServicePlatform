using System;
using System.Linq;
using System.Linq.Expressions;
using HomeServicePlatform.Domain.Modules.Operations.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class CommissionQuery
    {
        public static Expression<Func<Commission, bool>> IsActiveAt(DateTimeOffset now) =>
            c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now);

        public static Expression<Func<Commission, bool>> OverlapsWith(DateTimeOffset from, DateTimeOffset? to) =>
            c => (c.EffectiveTo == null || c.EffectiveTo > from)
              && (to == null || c.EffectiveFrom < to);

        public static IQueryable<Commission> ActiveAt(
            this IQueryable<Commission> source, DateTimeOffset now) =>
            source.Where(IsActiveAt(now))
                  .OrderByDescending(c => c.EffectiveFrom);
    }
}
