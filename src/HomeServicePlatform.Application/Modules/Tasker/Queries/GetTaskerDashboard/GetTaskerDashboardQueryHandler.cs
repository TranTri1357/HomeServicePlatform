using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
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

            // Danh sách việc có lịch hôm nay (chưa hủy, đã chốt) — vừa để đếm vừa để xem nhanh.
            var todayJobs = await _context.BookingItems
                .AsNoTracking()
                .Where(bi => bi.TaskerId == request.TaskerId
                    && bi.Status != Cancelled
                    && bi.StartAt >= todayStartUtc && bi.StartAt < todayEndUtc
                    && _context.Payments.Any(p => p.BookingId == bi.BookingId
                        && (p.Status == (short)PaymentStatus.Paid
                            || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))))
                .OrderBy(bi => bi.StartAt)
                .Select(bi => new TaskerTodayJobDto(
                    bi.BookingItemId,
                    bi.BookingId,
                    bi.Service.Name,
                    _context.BookingAddresses.Where(a => a.BookingId == bi.BookingId).Select(a => a.FullName).FirstOrDefault()
                        ?? bi.Booking.Customer.FullName,
                    bi.StartAt,
                    bi.TotalPrice,
                    (short)bi.Booking.Status,
                    _context.BookingAddresses.Where(a => a.BookingId == bi.BookingId).Select(a => a.AddressLine).FirstOrDefault()
                        ?? "Chưa cập nhật địa chỉ"))
                .ToListAsync(ct);

            var todayJobsCount = todayJobs.Count;

            // Tổng số ĐƠN đã chốt của thợ (mọi trạng thái) — nhãn "X việc" ở trang chủ.
            var totalJobsCount = await _context.Bookings.CountAsync(b =>
                b.BookingItems.Any(i => i.TaskerId == request.TaskerId)
                && _context.Payments.Any(p => p.BookingId == b.BookingId
                    && (p.Status == (short)PaymentStatus.Paid
                        || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))), ct);

            // Cấu hình hoa hồng còn hiệu lực (nạp một lần, phân giải trong bộ nhớ).
            var now = DateTimeOffset.UtcNow;
            var commissions = await _context.Commissions
                .AsNoTracking()
                .Where(c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now))
                .ToListAsync(ct);

            decimal NetOf(long serviceId, decimal gross) =>
                CommissionResolver.NetOf(gross, CommissionResolver.ResolveRate(commissions, serviceId, request.TaskerId, now));

            // Doanh thu tháng (các việc hoàn thành): gộp, thực nhận và hoa hồng.
            var monthItems = await _context.BookingItems
                .Where(bi => bi.TaskerId == request.TaskerId && bi.Status == Completed && bi.StartAt >= monthStartUtc)
                .Select(bi => new { bi.ServiceId, bi.TotalPrice })
                .ToListAsync(ct);

            decimal monthGross = 0m, monthEarnings = 0m;
            foreach (var it in monthItems)
            {
                monthGross += it.TotalPrice;
                monthEarnings += NetOf(it.ServiceId, it.TotalPrice);
            }
            var monthCommission = monthGross - monthEarnings;

            // Thực nhận 7 ngày (gom theo ngày VN trong bộ nhớ).
            var weekItems = await _context.BookingItems
                .Where(bi => bi.TaskerId == request.TaskerId && bi.Status == Completed && bi.StartAt >= weekStartUtc)
                .Select(bi => new { bi.StartAt, bi.ServiceId, bi.TotalPrice })
                .ToListAsync(ct);

            var buckets = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++) buckets[weekStartVn.AddDays(i)] = 0m;

            decimal todayEarnings = 0m;
            foreach (var it in weekItems)
            {
                var net = NetOf(it.ServiceId, it.TotalPrice);
                var d = it.StartAt.ToOffset(VnOffset).Date;
                if (buckets.ContainsKey(d)) buckets[d] += net;
                if (d == todayVn) todayEarnings += net;
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
                monthGross,
                monthCommission,
                weekly,
                totalJobsCount,
                todayJobs);

            return ApiResponse<TaskerDashboardDto>.Success(dto, "Lấy thống kê trang chủ thợ thành công.");
        }
    }
}
