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

            var nowVn = DateTimeOffset.UtcNow.ToOffset(VnOffset);
            var todayVn = nowVn.Date;
            var weekStartVn = todayVn.AddDays(-6);
            var monthStartVn = new DateTime(todayVn.Year, todayVn.Month, 1);

            var todayStartUtc = new DateTimeOffset(todayVn, VnOffset).ToUniversalTime();
            var todayEndUtc = new DateTimeOffset(todayVn.AddDays(1), VnOffset).ToUniversalTime();
            var weekStartUtc = new DateTimeOffset(weekStartVn, VnOffset).ToUniversalTime();
            var monthStartUtc = new DateTimeOffset(monthStartVn, VnOffset).ToUniversalTime();

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

            var totalJobsCount = await _context.Bookings.CountAsync(b =>
                b.BookingItems.Any(i => i.TaskerId == request.TaskerId)
                && _context.Payments.Any(p => p.BookingId == b.BookingId
                    && (p.Status == (short)PaymentStatus.Paid
                        || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))), ct);


            var periodStartUtc = weekStartUtc < monthStartUtc ? weekStartUtc : monthStartUtc;

            var periodItems = await _context.BookingItems
                .AsNoTracking()
                .Where(bi => bi.TaskerId == request.TaskerId && bi.Status == Completed && bi.StartAt >= periodStartUtc)
                .Select(bi => new { bi.BookingId, bi.TotalPrice, bi.StartAt })
                .ToListAsync(ct);

            var periodBookingIds = periodItems.Select(i => i.BookingId).Distinct().ToList();

            var creditedByBooking = await _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.Wallet.UserId == request.TaskerId
                            && t.Type == (short)WalletTransactionType.Earning
                            && t.ReferenceId != null
                            && periodBookingIds.Contains(t.ReferenceId.Value))
                .GroupBy(t => t.ReferenceId!.Value)
                .Select(g => new { BookingId = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Amount, ct);

            var settledByBooking = await _context.Payments
                .AsNoTracking()
                .Where(p => periodBookingIds.Contains(p.BookingId) && p.Status == (short)PaymentStatus.Paid)
                .GroupBy(p => p.BookingId)
                .Select(g => new { BookingId = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Amount, ct);

            var grossByBooking = periodItems
                .GroupBy(i => i.BookingId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalPrice));

            decimal ActualNetOfBooking(long bookingId)
            {
                var gross = grossByBooking.TryGetValue(bookingId, out var g) ? g : 0m;
                creditedByBooking.TryGetValue(bookingId, out var credited);
                settledByBooking.TryGetValue(bookingId, out var settled);
                return credited + Math.Max(0m, gross - settled);
            }

            decimal NetOfItem(long bookingId, decimal itemGross)
            {
                var gross = grossByBooking.TryGetValue(bookingId, out var g) ? g : 0m;
                if (gross <= 0m) return 0m;
                return Math.Round(ActualNetOfBooking(bookingId) * (itemGross / gross), 0, MidpointRounding.AwayFromZero);
            }

            decimal monthGross = 0m, monthEarnings = 0m;
            foreach (var it in periodItems.Where(i => i.StartAt >= monthStartUtc))
            {
                monthGross += it.TotalPrice;
                monthEarnings += NetOfItem(it.BookingId, it.TotalPrice);
            }
            var monthCommission = monthGross - monthEarnings;

            var buckets = new Dictionary<DateTime, decimal>();
            for (int i = 0; i < 7; i++) buckets[weekStartVn.AddDays(i)] = 0m;

            decimal todayEarnings = 0m;
            foreach (var it in periodItems.Where(i => i.StartAt >= weekStartUtc))
            {
                var net = NetOfItem(it.BookingId, it.TotalPrice);
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
