using System;
using System.Collections.Generic;
using System.Linq;
using HomeServicePlatform.Domain.Modules.Operations.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
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

        public static decimal NetOf(decimal gross, decimal ratePercent) =>
            Math.Round(gross - CommissionOf(gross, ratePercent), 0, MidpointRounding.AwayFromZero);

        public static decimal CommissionOf(decimal gross, decimal ratePercent) =>
            Math.Round(gross * ratePercent / 100m, 0, MidpointRounding.AwayFromZero);

        private static bool IsActive(Commission c, DateTimeOffset at) =>
            c.EffectiveFrom <= at && (c.EffectiveTo == null || c.EffectiveTo > at);
    }
}
