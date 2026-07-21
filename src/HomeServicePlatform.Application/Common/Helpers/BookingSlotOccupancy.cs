using System;
using System.Linq.Expressions;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class BookingSlotOccupancy
    {
        public static readonly TimeSpan HoldTtl = TimeSpan.FromMinutes(15);

        public static DateTimeOffset FreshHoldSince(DateTimeOffset now) => now - HoldTtl;

        public static Expression<Func<BookingItem, bool>> Occupying(DateTimeOffset freshHoldSince)
            => b => b.Status != (short)BookingStatus.Cancelled
                    && (b.Status != (short)BookingStatus.Pending || b.CreatedAt > freshHoldSince);
    }
}
