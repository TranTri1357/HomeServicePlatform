using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability
{
    public class GetTaskerAvailabilityQueryHandler
        : IRequestHandler<GetTaskerAvailabilityQuery, ApiResponse<TaskerAvailabilityDto>>
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);
        private static readonly TimeSpan HoldTtl = TimeSpan.FromMinutes(15);

        private readonly IApplicationDbContext _context;
        private readonly BufferPolicyOptions _buffer;

        public GetTaskerAvailabilityQueryHandler(IApplicationDbContext context, IOptions<BufferPolicyOptions> buffer)
        {
            _context = context;
            _buffer = buffer.Value;
        }

        public async Task<ApiResponse<TaskerAvailabilityDto>> Handle(GetTaskerAvailabilityQuery request, CancellationToken ct)
        {
            var date = request.Date;
            short dayOfWeek = (short)date.DayOfWeek;

            var schedule = await _context.TaskerSchedules.AsNoTracking()
                .FirstOrDefaultAsync(s => s.TaskerId == request.TaskerId && s.DayOfWeek == dayOfWeek, ct);

            if (schedule == null)
                return ApiResponse<TaskerAvailabilityDto>.Success(
                    new TaskerAvailabilityDto(date, false, new List<AvailabilitySlotDto>()),
                    "Thợ không làm việc vào ngày này.");

            var startOfDay = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, VnOffset);
            var startOfDayUtc = startOfDay.ToUniversalTime();
            var endOfDayUtc = startOfDay.AddDays(1).ToUniversalTime();
            var now = DateTimeOffset.UtcNow;
            var freshHoldSince = now - HoldTtl;

            var timeOffs = await _context.TaskerTimeOffs.AsNoTracking()
                .Where(t => t.TaskerId == request.TaskerId && t.StartAt < endOfDayUtc && t.EndAt > startOfDayUtc)
                .Select(t => new { t.StartAt, t.EndAt })
                .ToListAsync(ct);

            // Đơn "chiếm chỗ": đã nhận/đang làm (1,2,3) HOẶC đơn giữ chỗ (0) còn trong hạn TTL.
            // Lấy kèm TỌA ĐỘ địa điểm của từng đơn (qua BookingAddress.Geom) để tính buffer di chuyển.
            var busy = await _context.BookingItems.AsNoTracking()
                .Where(b => b.TaskerId == request.TaskerId
                            && b.StartAt < endOfDayUtc && b.EndAt > startOfDayUtc
                            && ((b.Status >= 1 && b.Status <= 3)
                                || (b.Status == 0 && b.CreatedAt > freshHoldSince)))
                .Select(b => new
                {
                    b.StartAt,
                    b.EndAt,
                    Geom = b.Booking.BookingAddress != null ? b.Booking.BookingAddress.Geom : null
                })
                .ToListAsync(ct);

            // Với mỗi đơn bận, tính buffer (phút) từ địa điểm đơn đó tới ĐÍCH của đơn sắp đặt.
            // Không có tọa độ đích ⇒ không trừ buffer (giữ hành vi cũ, để backend chặn khi tạo đơn).
            bool hasDest = request.Lat.HasValue && request.Lng.HasValue;
            var busyPadded = busy
                .Select(b =>
                {
                    double? lat = b.Geom?.Y;
                    double? lng = b.Geom?.X;
                    int bufferMin = hasDest
                        ? TravelBufferCalculator.BufferMinutesBetween(lat, lng, request.Lat, request.Lng, _buffer)
                        : 0;
                    var pad = TimeSpan.FromMinutes(bufferMin);
                    return new { Start = b.StartAt - pad, End = b.EndAt + pad };
                })
                .ToList();

            var slots = new List<AvailabilitySlotDto>();
            var current = schedule.StartTime;
            while (current < schedule.EndTime)
            {
                var slotStart = startOfDay.Add(current.ToTimeSpan());
                var slotEnd = slotStart.AddHours(1);

                bool blocked =
                    timeOffs.Any(t => t.StartAt < slotEnd && t.EndAt > slotStart) ||
                    busyPadded.Any(b => b.Start < slotEnd && b.End > slotStart);

                slots.Add(new AvailabilitySlotDto(current, !blocked));
                current = current.AddHours(1);
            }

            return ApiResponse<TaskerAvailabilityDto>.Success(
                new TaskerAvailabilityDto(date, true, slots),
                "Lấy khung giờ trống của thợ thành công.");
        }
    }
}
