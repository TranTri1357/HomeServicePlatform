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
    /// <summary>
    /// Bộ kiểm tra "thời gian đệm di chuyển" ở tầng ứng dụng: đảm bảo thợ kịp di chuyển
    /// từ đơn liền trước tới địa điểm đơn mới (và từ đơn mới tới đơn liền sau).
    ///
    /// Đây là tầng buffer ĐỘNG theo khoảng cách (mạnh hơn sàn cứng của ràng buộc CSDL).
    /// Gọi trong transaction SAU khi đã giữ advisory lock theo thợ để tránh hai đơn đồng thời
    /// cùng lọt qua (chống double-booking có tính tới quãng đường).
    /// </summary>
    public static class TravelBufferGuard
    {

        private sealed class NeighborProj
        {
            public DateTimeOffset At { get; set; }
            public Point? Geom { get; set; }
        }

        /// <summary>
        /// Ném <see cref="BadRequestException"/> nếu khoảng trống tới đơn liền trước/sau của thợ
        /// nhỏ hơn thời gian đệm cần để di chuyển tới/từ địa điểm đơn mới.
        /// </summary>
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

            // Đơn liền TRƯỚC: đơn đang chiếm khung giờ (xem BookingSlotOccupancy) kết thúc muộn
            // nhất mà vẫn không muộn hơn giờ bắt đầu đơn mới.
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

            // Đơn liền SAU: đơn đang chiếm khung giờ bắt đầu sớm nhất mà không sớm hơn giờ kết thúc đơn mới.
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

        // Đổi mốc UTC sang giờ VN (UTC+7) để thông báo cho khách dễ đọc.
        private static DateTimeOffset ToVn(DateTimeOffset utc) => utc.ToOffset(TimeSpan.FromHours(7));
    }
}
