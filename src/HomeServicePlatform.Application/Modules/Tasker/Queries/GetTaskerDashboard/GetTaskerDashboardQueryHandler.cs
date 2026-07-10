using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard
{
    public class GetTaskerDashboardQueryHandler
        : IRequestHandler<GetTaskerDashboardQuery, ApiResponse<TaskerDashboardDto>>
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);
        private const short Completed = (short)BookingStatus.Completed;
        private const short Cancelled = (short)BookingStatus.Cancelled;

        private readonly IApplicationDbContext _context;

        public GetTaskerDashboardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerDashboardDto>> Handle(GetTaskerDashboardQuery request, CancellationToken ct)
        {
            var profile = await _context.TaskerProfiles
                .AsNoTracking()
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted, ct);

            if (profile == null)
                throw new NotFoundException("Không tìm thấy hồ sơ thợ.");

            // Mốc thời gian theo giờ Việt Nam (UTC+7), đổi về UTC để truy vấn.
            var nowVn = DateTimeOffset.UtcNow.ToOffset(VnOffset);
            var todayVn = nowVn.Date;
            var weekStartVn = todayVn.AddDays(-6);
            var monthStartVn = new DateTime(todayVn.Year, todayVn.Month, 1);

            var todayStartUtc = new DateTimeOffset(todayVn, VnOffset).ToUniversalTime();
            var todayEndUtc = new DateTimeOffset(todayVn.AddDays(1), VnOffset).ToUniversalTime();
            var weekStartUtc = new DateTimeOffset(weekStartVn, VnOffset).ToUniversalTime();
            var monthStartUtc = new DateTimeOffset(monthStartVn, VnOffset).ToUniversalTime();

            // Số việc có lịch hôm nay (chưa hủy).
            var todayJobsCount = await _context.BookingItems.CountAsync(bi =>
                bi.TaskerId == request.TaskerId
                && bi.Status != Cancelled
                && bi.StartAt >= todayStartUtc && bi.StartAt < todayEndUtc, ct);

            // Doanh thu tháng (các việc hoàn thành).
            var monthEarnings = await _context.BookingItems
                .Where(bi => bi.TaskerId == request.TaskerId && bi.Status == Completed && bi.StartAt >= monthStartUtc)
                .SumAsync(bi => (decimal?)bi.TotalPrice, ct) ?? 0m;

            // Doanh thu 7 ngày (gom theo ngày VN trong bộ nhớ).
            var weekItems = await _context.BookingItems
                .Where(bi => bi.TaskerId == request.TaskerId && bi.Status == Completed && bi.StartAt >= weekStartUtc)
                .Select(bi => new { bi.StartAt, bi.TotalPrice })
                .ToListAsync(ct);

            var buckets = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++) buckets[weekStartVn.AddDays(i)] = 0m;

            decimal todayEarnings = 0m;
            foreach (var it in weekItems)
            {
                var d = it.StartAt.ToOffset(VnOffset).Date;
                if (buckets.ContainsKey(d)) buckets[d] += it.TotalPrice;
                if (d == todayVn) todayEarnings += it.TotalPrice;
            }

            var weekly = buckets
                .OrderBy(kv => kv.Key)
                .Select(kv => new DailyRevenueDto(kv.Key, kv.Value))
                .ToList();

            var dto = new TaskerDashboardDto(
                profile.User.FullName,
                profile.Status == 1,
                profile.RatingAvg,
                profile.TotalReviews,
                todayEarnings,
                todayJobsCount,
                monthEarnings,
                weekly);

            return ApiResponse<TaskerDashboardDto>.Success(dto, "Lấy thống kê trang chủ thợ thành công.");
        }
    }
}
