using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class TravelBufferGuard
    {

        private sealed class NeighborProj
        {
            public DateTimeOffset At { get; set; }
            public Point? Geom { get; set; }
        }

        public static async Task EnsureTravelFeasibleAsync(
            IApplicationDbContext context,
            BufferPolicyOptions options,
            long taskerId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            double? destLat,
            double? destLng,
            CancellationToken ct)
        {
            var freshHoldSince = BookingSlotOccupancy.FreshHoldSince(DateTimeOffset.UtcNow);

            var prev = await context.BookingItems.AsNoTracking()
                .Where(b => b.TaskerId == taskerId && b.EndAt <= startAt)
                .Where(BookingSlotOccupancy.Occupying(freshHoldSince))
                .OrderByDescending(b => b.EndAt)
                .Select(b => new NeighborProj
                {
                    At = b.EndAt,
                    Geom = b.Booking.BookingAddress != null ? b.Booking.BookingAddress.Geom : null
                })
                .FirstOrDefaultAsync(ct);

            if (prev != null)
            {
                int buffer = TravelBufferCalculator.BufferMinutesBetween(prev.Geom?.Y, prev.Geom?.X, destLat, destLng, options);
                double gapMin = (startAt - prev.At).TotalMinutes;
                if (gapMin < buffer)
                    throw new BadRequestException(
                        $"Thợ có đơn kết thúc lúc {ToVn(prev.At):HH:mm}, cần khoảng {buffer} phút để di chuyển tới địa điểm của bạn. " +
                        $"Vui lòng chọn khung giờ bắt đầu từ {ToVn(prev.At.AddMinutes(buffer)):HH:mm} trở đi.");
            }

            var next = await context.BookingItems.AsNoTracking()
                .Where(b => b.TaskerId == taskerId && b.StartAt >= endAt)
                .Where(BookingSlotOccupancy.Occupying(freshHoldSince))
                .OrderBy(b => b.StartAt)
                .Select(b => new NeighborProj
                {
                    At = b.StartAt,
                    Geom = b.Booking.BookingAddress != null ? b.Booking.BookingAddress.Geom : null
                })
                .FirstOrDefaultAsync(ct);

            if (next != null)
            {
                int buffer = TravelBufferCalculator.BufferMinutesBetween(destLat, destLng, next.Geom?.Y, next.Geom?.X, options);
                double gapMin = (next.At - endAt).TotalMinutes;
                if (gapMin < buffer)
                    throw new BadRequestException(
                        $"Thợ có đơn kế tiếp lúc {ToVn(next.At):HH:mm}, cần khoảng {buffer} phút di chuyển sau khi làm xong cho bạn. " +
                        $"Vui lòng chọn khung giờ kết thúc trước {ToVn(next.At.AddMinutes(-buffer)):HH:mm}.");
            }
        }

        private static DateTimeOffset ToVn(DateTimeOffset utc) => utc.ToOffset(TimeSpan.FromHours(7));
    }
}
