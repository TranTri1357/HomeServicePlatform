using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Admin.Queries.GetAdminDashboard
{
    public class GetAdminDashboardQueryHandler
        : IRequestHandler<GetAdminDashboardQuery, ApiResponse<AdminDashboardDto>>
    {
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        private readonly IApplicationDbContext _context;
        public GetAdminDashboardQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<AdminDashboardDto>> Handle(GetAdminDashboardQuery request, CancellationToken ct)
        {
            var nowVn = DateTimeOffset.UtcNow.ToOffset(VnOffset);
            var todayVn = nowVn.Date;
            var weekStartVn = todayVn.AddDays(-6);

            var todayStartUtc = new DateTimeOffset(todayVn, VnOffset).ToUniversalTime();
            var todayEndUtc = new DateTimeOffset(todayVn.AddDays(1), VnOffset).ToUniversalTime();
            var weekStartUtc = new DateTimeOffset(weekStartVn, VnOffset).ToUniversalTime();

            // ── KPI ────────────────────────────────────────────────────────────
            var todayBookings = await _context.Bookings
                .CountAsync(b => b.CreatedAt >= todayStartUtc && b.CreatedAt < todayEndUtc, ct);

            var todayRevenue = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= todayStartUtc && b.CreatedAt < todayEndUtc)
                .SumAsync(b => (decimal?)b.FinalAmount, ct) ?? 0m;

            var totalCustomers = await _context.UserRoles
                .Where(ur => ur.Role.RoleName == "Customer" && !ur.User.IsDeleted)
                .Select(ur => ur.UserId)
                .Distinct()
                .CountAsync(ct);

            var totalTaskers = await _context.TaskerProfiles.CountAsync(t => !t.IsDeleted, ct);
            var activeTaskers = await _context.TaskerProfiles.CountAsync(t => !t.IsDeleted && t.Status == 1, ct);
            var pendingTaskers = await _context.TaskerProfiles.CountAsync(t => !t.IsDeleted && t.Status == 0, ct);

            var totalBookings = await _context.Bookings.CountAsync(ct);
            var totalRevenue = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed)
                .SumAsync(b => (decimal?)b.FinalAmount, ct) ?? 0m;

            var openDisputes = await _context.Disputes.CountAsync(d => d.Status == 0, ct);

            // ── Doanh thu 7 ngày (gom theo ngày VN trong bộ nhớ) ────────────────
            var weekItems = await _context.Bookings
                .Where(b => b.Status == BookingStatus.Completed && b.CreatedAt >= weekStartUtc)
                .Select(b => new { b.CreatedAt, b.FinalAmount })
                .ToListAsync(ct);

            var buckets = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++) buckets[weekStartVn.AddDays(i)] = 0m;
            foreach (var it in weekItems)
            {
                var d = it.CreatedAt.ToOffset(VnOffset).Date;
                if (buckets.ContainsKey(d)) buckets[d] += it.FinalAmount;
            }
            var weeklyRevenue = buckets
                .OrderBy(kv => kv.Key)
                .Select(kv => new DailyRevenuePoint(kv.Key, kv.Value))
                .ToList();

            // ── Đơn theo trạng thái ─────────────────────────────────────────────
            var statusGroups = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToListAsync(ct);
            var bookingsByStatus = statusGroups
                .Select(x => new StatusCountDto((short)x.Key, x.Count))
                .OrderBy(x => x.Status)
                .ToList();

            // ── 5 đơn gần nhất ──────────────────────────────────────────────────
            var recentBookings = await _context.Bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .Select(b => new RecentBookingDto(
                    b.BookingId,
                    b.Customer.FullName,
                    b.FinalAmount,
                    (short)b.Status,
                    b.CreatedAt))
                .ToListAsync(ct);

            var dto = new AdminDashboardDto(
                todayRevenue,
                todayBookings,
                totalCustomers,
                totalTaskers,
                activeTaskers,
                pendingTaskers,
                totalBookings,
                totalRevenue,
                openDisputes,
                weeklyRevenue,
                bookingsByStatus,
                recentBookings);

            return ApiResponse<AdminDashboardDto>.Success(dto, "Lấy số liệu dashboard thành công.");
        }
    }
}
