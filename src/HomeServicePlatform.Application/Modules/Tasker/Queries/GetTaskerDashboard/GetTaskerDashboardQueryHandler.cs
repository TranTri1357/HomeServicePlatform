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

            // 💰 THU NHẬP ĐỌC TỪ SỔ VÍ — KHÔNG TÍNH LẠI TỪ BIỂU PHÍ HOA HỒNG.
            //
            // Trước đây chỗ này gọi CommissionResolver.ResolveRate(..., now) rồi áp tỷ lệ HÔM NAY
            // lên các đơn ĐÃ HOÀN THÀNH trong quá khứ. Nhưng CompleteWork đã ghi ví bằng tỷ lệ
            // TẠI THỜI ĐIỂM HOÀN THÀNH. Hậu quả: admin đổi hoa hồng 10%→20% giữa tháng là dashboard
            // hiển thị lại cả tháng theo 20%, lệch hẳn với số tiền thật nằm trong ví — thợ thấy
            // "thu nhập tự nhiên tụt" trong khi ví không hề đổi.
            //
            // Nay lấy số ĐÃ CHỐT. Thực nhận của một đơn gồm hai nguồn:
            //   (1) tiền ví đã ghi có  — bút toán Earning do CompleteWork tạo (đã trừ hoa hồng);
            //   (2) tiền mặt khách trả thẳng cho thợ — phần giá đơn KHÔNG đi qua hệ thống
            //       (gross trừ đi số đã thanh toán qua sàn). Sàn không giữ nên cũng không thu
            //       hoa hồng phần này, thợ cầm trọn.
            // Công thức này đúng cho cả ba kiểu: trả trước toàn bộ, cọc 30%, và tiền mặt 100%.

            // Nạp một lượt các hạng mục đã hoàn thành trong khoảng cần thống kê. Lưu ý mốc tuần
            // có thể rơi sang tháng trước (vd hôm nay mùng 3) nên phải lấy mốc SỚM HƠN.
            var periodStartUtc = weekStartUtc < monthStartUtc ? weekStartUtc : monthStartUtc;

            var periodItems = await _context.BookingItems
                .AsNoTracking()
                .Where(bi => bi.TaskerId == request.TaskerId && bi.Status == Completed && bi.StartAt >= periodStartUtc)
                .Select(bi => new { bi.BookingId, bi.TotalPrice, bi.StartAt })
                .ToListAsync(ct);

            var periodBookingIds = periodItems.Select(i => i.BookingId).Distinct().ToList();

            // (1) Tiền THỰC SỰ đã ghi có vào ví thợ, gom theo đơn.
            var creditedByBooking = await _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.Wallet.UserId == request.TaskerId
                            && t.Type == (short)WalletTransactionType.Earning
                            && t.ReferenceId != null
                            && periodBookingIds.Contains(t.ReferenceId.Value))
                .GroupBy(t => t.ReferenceId!.Value)
                .Select(g => new { BookingId = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Amount, ct);

            // (2) Tiền khách đã trả QUA HỆ THỐNG, gom theo đơn (phần còn lại là tiền mặt trao tay).
            var settledByBooking = await _context.Payments
                .AsNoTracking()
                .Where(p => periodBookingIds.Contains(p.BookingId) && p.Status == (short)PaymentStatus.Paid)
                .GroupBy(p => p.BookingId)
                .Select(g => new { BookingId = g.Key, Amount = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Amount, ct);

            var grossByBooking = periodItems
                .GroupBy(i => i.BookingId)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.TotalPrice));

            // Thực nhận của cả đơn = tiền ví đã ghi có + phần khách trả tiền mặt trực tiếp.
            decimal ActualNetOfBooking(long bookingId)
            {
                var gross = grossByBooking.TryGetValue(bookingId, out var g) ? g : 0m;
                creditedByBooking.TryGetValue(bookingId, out var credited);
                settledByBooking.TryGetValue(bookingId, out var settled);
                return credited + Math.Max(0m, gross - settled);
            }

            // Bút toán ví ghi theo ĐƠN, còn biểu đồ gom theo NGÀY của từng hạng mục — nên phân bổ
            // thực nhận về các hạng mục theo tỷ trọng giá.
            decimal NetOfItem(long bookingId, decimal itemGross)
            {
                var gross = grossByBooking.TryGetValue(bookingId, out var g) ? g : 0m;
                if (gross <= 0m) return 0m;
                return Math.Round(ActualNetOfBooking(bookingId) * (itemGross / gross), 0, MidpointRounding.AwayFromZero);
            }

            // Doanh thu tháng (các việc hoàn thành): gộp, thực nhận và hoa hồng.
            decimal monthGross = 0m, monthEarnings = 0m;
            foreach (var it in periodItems.Where(i => i.StartAt >= monthStartUtc))
            {
                monthGross += it.TotalPrice;
                monthEarnings += NetOfItem(it.BookingId, it.TotalPrice);
            }
            // Hoa hồng = phần chênh giữa giá đơn và số thợ thực nhận (suy ra từ tiền thật, không tính lại).
            var monthCommission = monthGross - monthEarnings;

            // Thực nhận 7 ngày (gom theo ngày VN trong bộ nhớ).
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
