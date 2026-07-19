using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HomeServicePlatform.Api.BackgroundJobs
{
    /// <summary>
    /// Job nền định kỳ nhả "chỗ giữ" của các đơn hết hạn để trả slot cho người khác đặt.
    /// Trước đây việc này chạy nội tuyến trong CreateBooking (chỉ chạy khi có người đặt đơn mới,
    /// và làm chậm đường đặt đơn). Tách ra chạy độc lập giúp slot luôn được nhả đúng hạn.
    ///  • Đơn thường Pending quá 15 phút mà chưa chốt thanh toán → hủy.
    ///  • Đơn khẩn cấp Pending đã quá hạn (EmergencyExpiresAt ~30s) mà chưa ai nhận → hủy.
    /// </summary>
    public class ExpiredBookingCleanupService : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromMinutes(2);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ExpiredBookingCleanupService> _logger;

        public ExpiredBookingCleanupService(
            IServiceScopeFactory scopeFactory, ILogger<ExpiredBookingCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Chờ một chút cho app khởi động xong rồi mới chạy vòng đầu.
            try { await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); }
            catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Interval);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SweepAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Không để lỗi một vòng làm chết cả job — ghi log rồi chờ vòng sau.
                    _logger.LogError(ex, "Lỗi khi dọn đơn đặt lịch hết hạn.");
                }

                try
                {
                    if (!await timer.WaitForNextTickAsync(stoppingToken)) break;
                }
                catch (OperationCanceledException) { break; }
            }
        }

        private async Task SweepAsync(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

            var nowUtc = DateTimeOffset.UtcNow;
            // Dùng chung mốc TTL với BookingSlotOccupancy: nếu hai nơi lệch nhau thì sẽ có đơn
            // vừa bị coi là hết hạn giữ chỗ, vừa vẫn bị tính là đang chiếm khung giờ (hoặc ngược lại).
            var expiredThreshold = BookingSlotOccupancy.FreshHoldSince(nowUtc);

            // "Đã chốt" = có thanh toán thành công (Paid) HOẶC đơn tiền mặt (Pending + Cash).
            // Chỉ nhả những đơn thường Pending quá hạn mà CHƯA chốt.
            var expiredHolds = await context.Bookings
                .Include(b => b.BookingItems)
                .Where(b => b.Status == BookingStatus.Pending
                            && !b.IsEmergency
                            && b.CreatedAt < expiredThreshold
                            && !context.Payments.Any(p => p.BookingId == b.BookingId
                                && (p.Status == (short)PaymentStatus.Paid
                                    || (p.Status == (short)PaymentStatus.Pending
                                        && p.Method == (short)PaymentMethod.Cash))))
                .ToListAsync(ct);

            // Đơn khẩn cấp Pending đã quá cửa sổ nhận (EmergencyExpiresAt) mà chưa ai nhận.
            var expiredEmergencies = await context.Bookings
                .Include(b => b.BookingItems)
                .Where(b => b.Status == BookingStatus.Pending
                            && b.IsEmergency
                            && b.EmergencyExpiresAt != null
                            && b.EmergencyExpiresAt < nowUtc)
                .ToListAsync(ct);

            var toCancel = expiredHolds.Concat(expiredEmergencies).ToList();
            if (toCancel.Count == 0) return;

            foreach (var booking in toCancel)
            {
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = nowUtc;
                foreach (var item in booking.BookingItems)
                {
                    item.Status = (short)BookingStatus.Cancelled;
                    item.UpdatedAt = nowUtc;
                }
            }

            await context.SaveChangesAsync(ct);
            _logger.LogInformation("Đã hủy {Count} đơn đặt lịch hết hạn.", toCancel.Count);
        }
    }
}
