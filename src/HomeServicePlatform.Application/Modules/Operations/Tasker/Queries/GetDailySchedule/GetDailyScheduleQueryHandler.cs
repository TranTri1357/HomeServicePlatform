using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetDailySchedule
{
    public class GetDailyScheduleQueryHandler : IRequestHandler<GetDailyScheduleQuery, ApiResponse<DailyScheduleDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetDailyScheduleQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<DailyScheduleDto>> Handle(GetDailyScheduleQuery request, CancellationToken ct)
        {
            var taskerProfile = await _context.TaskerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId, ct);
            if (taskerProfile == null) throw new Exception("Bạn chưa hoàn thiện hồ sơ thợ.");

            var actualTaskerId = taskerProfile.TaskerProfileId;
            var result = new DailyScheduleDto { Date = request.Date };

            var startOfDay = new DateTimeOffset(request.Date.Year, request.Date.Month, request.Date.Day, 0, 0, 0, TimeSpan.FromHours(7));
            var endOfDay = startOfDay.AddDays(1);

            var startOfDayUtc = startOfDay.ToUniversalTime();
            var endOfDayUtc = endOfDay.ToUniversalTime();

            short dayOfWeek = (short)request.Date.DayOfWeek;

            var defaultSchedule = await _context.TaskerSchedules.AsNoTracking()
                .FirstOrDefaultAsync(s => s.TaskerId == actualTaskerId && s.DayOfWeek == dayOfWeek, ct);

            if (defaultSchedule == null) return ApiResponse<DailyScheduleDto>.Success(result, "Bạn không có lịch làm việc vào ngày này.");

            var timeOffs = await _context.TaskerTimeOffs.AsNoTracking()
                .Where(t => t.TaskerId == actualTaskerId && t.StartAt < endOfDayUtc && t.EndAt > startOfDayUtc).ToListAsync(ct);

            var bookings = await _context.BookingItems
                .Include(b => b.Service).Include(b => b.Booking).ThenInclude(b => b.Customer).Include(b => b.Booking.BookingAddress)
                .AsNoTracking()
                .Where(b => b.TaskerId == actualTaskerId && b.StartAt >= startOfDayUtc && b.StartAt < endOfDayUtc && (b.Status == 1 || b.Status == 2))
                .OrderBy(b => b.StartAt).ToListAsync(ct);

            var currentTime = defaultSchedule.StartTime;
            while (currentTime < defaultSchedule.EndTime)
            {
                var slotDT = startOfDay.Add(currentTime.ToTimeSpan());
                var nextSlotDT = slotDT.AddHours(1);
                short slotStatus = 0;

                if (timeOffs.Any(t => t.StartAt < nextSlotDT && t.EndAt > slotDT)) slotStatus = 2;
                else if (bookings.Any(b => b.StartAt < nextSlotDT && b.EndAt > slotDT)) slotStatus = 1;

                result.TimeSlots.Add(new TimeSlotDto { Time = currentTime, Status = slotStatus });
                currentTime = currentTime.AddHours(1);
            }

            result.UpcomingJobs = bookings.Select(b => new UpcomingJobDto
            {
                BookingItemId = b.BookingItemId,
                ServiceName = b.Service.Name,
                CustomerName = b.Booking.Customer.FullName,
                AddressLine = b.Booking.BookingAddress?.AddressLine ?? "Không có địa chỉ",
                StartTime = TimeOnly.FromDateTime(b.StartAt.DateTime),
                EndTime = TimeOnly.FromDateTime(b.EndAt.DateTime),
                JobStatus = b.Status
            }).ToList();

            return ApiResponse<DailyScheduleDto>.Success(result, "Lấy lịch làm việc thành công.");
        }
    }
}
