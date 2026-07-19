using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeServicePlatform.Api.BackgroundJobs
{
    /// <summary>
    /// Job nền định kỳ đóng các đơn "treo" và trả lại những gì đang bị giữ (slot và TIỀN).
    /// Trước đây việc này chạy nội tuyến trong CreateBooking (chỉ chạy khi có người đặt đơn mới,
    /// và làm chậm đường đặt đơn). Tách ra chạy độc lập giúp slot luôn được nhả đúng hạn.
    ///
    ///  • Đơn thường Pending quá 15 phút mà CHƯA chốt thanh toán → hủy (chỉ nhả chỗ, không có tiền).
    ///  • Đơn khẩn cấp Pending đã quá hạn (EmergencyExpiresAt ~30s) mà chưa ai nhận → hủy.
    ///  • Đơn thường ĐÃ THU TIỀN mà thợ không xác nhận quá hạn → hủy + HOÀN 100% cho khách.
    ///
    /// Mọi nhánh đều gửi thông báo: trước đây job hủy đơn hoàn toàn im lặng nên khách chỉ thấy
    /// đơn tự chuyển thành "Đã hủy" mà không hiểu vì sao.
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

        /// <summary>Vì sao một đơn bị hủy — quyết định thông báo và có hoàn tiền hay không.</summary>
        private enum CancelReason
        {
            HoldExpired,        // Hết hạn giữ chỗ, chưa thu tiền
            EmergencyNoTasker,  // Đơn khẩn hết hạn, không thợ nào nhận
            NotConfirmedPaid    // Khách đã trả tiền nhưng thợ không xác nhận kịp hạn
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
            var policy = scope.ServiceProvider.GetRequiredService<IOptions<RefundPolicyOptions>>().Value;

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

            // 💸 Đơn khách ĐÃ TRẢ TIỀN nhưng thợ không bấm nhận trong hạn. Trước đây nhóm này bị
            // loại khỏi mọi nhánh dọn dẹp (điều kiện ở trên loại trừ đơn đã Paid) nên nằm Pending
            // vĩnh viễn và tiền khách kẹt lại trong ví ký quỹ, không đường ra.
            var confirmDeadline = nowUtc.AddMinutes(-Math.Max(1, policy.TaskerConfirmDeadlineMinutes));
            var unconfirmedPaid = await context.Bookings
                .Include(b => b.BookingItems)
                .Where(b => b.Status == BookingStatus.Pending
                            && !b.IsEmergency
                            && b.CreatedAt < confirmDeadline
                            && context.Payments.Any(p => p.BookingId == b.BookingId
                                                         && p.Status == (short)PaymentStatus.Paid))
                .ToListAsync(ct);

            var targets = expiredHolds.Select(b => (Booking: b, Reason: CancelReason.HoldExpired))
                .Concat(expiredEmergencies.Select(b => (Booking: b, Reason: CancelReason.EmergencyNoTasker)))
                .Concat(unconfirmedPaid.Select(b => (Booking: b, Reason: CancelReason.NotConfirmedPaid)))
                .ToList();

            if (targets.Count == 0) return;

            // Xử lý TỪNG ĐƠN một, mỗi đơn một transaction. Gộp cả lô vào một SaveChanges sẽ khiến
            // một đơn lỗi (vd ví ký quỹ thiếu số dư) kéo rollback toàn bộ lô và lặp lại mãi mỗi vòng.
            var cancelled = 0;
            foreach (var (booking, reason) in targets)
            {
                try
                {
                    if (await CancelOneAsync(scope, context, booking, reason, ct)) cancelled++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Không hủy được đơn BK{BookingId} ({Reason}).",
                        booking.BookingId, reason);
                }
            }

            if (cancelled > 0)
                _logger.LogInformation("Đã hủy {Count} đơn đặt lịch hết hạn.", cancelled);
        }

        private async Task<bool> CancelOneAsync(
            IServiceScope scope,
            IApplicationDbContext context,
            Domain.Modules.Bookings.Entities.Booking booking,
            CancelReason reason,
            CancellationToken ct)
        {
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var now = DateTimeOffset.UtcNow;

            await unitOfWork.BeginTransactionAsync();
            try
            {
                // 🔒 Khóa theo đơn rồi ĐỌC LẠI trạng thái: job có thể chạy đúng lúc thợ đang bấm nhận.
                // Không có bước này thì hai bên có thể cùng ghi và đơn vừa Accepted vừa bị hủy.
                await context.AcquireBookingClaimLockAsync(booking.BookingId, ct);

                var currentStatus = await context.Bookings.AsNoTracking()
                    .Where(b => b.BookingId == booking.BookingId)
                    .Select(b => (short)b.Status)
                    .FirstOrDefaultAsync(ct);
                if (currentStatus != (short)BookingStatus.Pending)
                {
                    // Thợ đã kịp nhận trong lúc job đang quét — bỏ qua, không đụng vào đơn.
                    await unitOfWork.RollbackTransactionAsync();
                    return false;
                }

                short oldStatus = (short)booking.Status;
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = now;
                foreach (var item in booking.BookingItems)
                {
                    item.Status = (short)BookingStatus.Cancelled;
                    item.UpdatedAt = now;
                }

                // Audit trail: ChangedBy = null nghĩa là HỆ THỐNG tự hủy, không phải người dùng nào.
                // Cột này có khóa ngoại sang users nên không được nhét id giả (kể cả 0).
                context.BookingHistories.Add(new BookingHistory
                {
                    BookingId = booking.BookingId,
                    OldStatus = oldStatus,
                    NewStatus = (short)BookingStatus.Cancelled,
                    ChangedBy = null,
                    CreatedAt = now
                });

                await NotifyAndRefundAsync(context, booking, reason, now, ct);

                await context.SaveChangesAsync(ct);
                await unitOfWork.CommitTransactionAsync();
                return true;
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private static async Task NotifyAndRefundAsync(
            IApplicationDbContext context,
            Domain.Modules.Bookings.Entities.Booking booking,
            CancelReason reason,
            DateTimeOffset now,
            CancellationToken ct)
        {
            var code = $"BK{booking.BookingId}";

            if (reason != CancelReason.NotConfirmedPaid)
            {
                // Hai nhánh này chưa có tiền vào hệ thống nên chỉ cần báo cho khách biết lý do.
                var body = reason == CancelReason.EmergencyNoTasker
                    ? $"Đơn khẩn cấp {code} đã hết hạn vì chưa có thợ nào nhận. Bạn có thể thử gọi lại."
                    : $"Đơn {code} đã tự hủy do quá hạn giữ chỗ mà chưa hoàn tất thanh toán.";

                context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId, NotificationType.BookingAutoCancelled, "Đơn đã tự hủy", body));
                return;
            }

            // 💸 Thợ không xác nhận = lỗi hoàn toàn không thuộc về khách, nên hoàn 100%:
            // KHÔNG áp RefundPolicy theo thời điểm hủy (chính sách đó dành cho khách chủ động hủy),
            // không phạt khách, và sàn cũng không giữ lại phí nào.
            var totalPaid = await context.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var outcome = await RefundExecutor.IssueRefundAsync(
                context, booking,
                refundAmount: totalPaid, penaltyAmount: 0m,
                RefundInitiator.System,
                $"Tự hủy đơn {code}: thợ không xác nhận trong thời hạn quy định.",
                now, ct);

            context.Notifications.Add(NotificationBuilder.Build(
                booking.CustomerId,
                NotificationType.BookingAutoCancelled,
                "Đơn đã tự hủy",
                $"Đơn {code} đã tự hủy vì thợ không xác nhận trong thời hạn quy định."));

            if (outcome.Executed && outcome.RefundedToCustomer > 0)
            {
                context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.RefundIssued,
                    "Đã hoàn tiền",
                    $"Hệ thống đã hoàn 100% ({outcome.RefundedToCustomer:#,##0}đ) của đơn {code} vào ví của bạn."));
            }

            // Báo cho thợ được chỉ định biết vì sao mất đơn — đây cũng là dữ liệu để đánh giá độ
            // tin cậy của thợ về sau.
            var taskerId = booking.BookingItems
                .Where(i => i.TaskerId.HasValue)
                .Select(i => i.TaskerId!.Value)
                .FirstOrDefault();

            if (taskerId > 0)
            {
                context.Notifications.Add(NotificationBuilder.Build(
                    taskerId,
                    NotificationType.BookingExpiredUnconfirmed,
                    "Đơn đã bị hủy",
                    $"Đơn {code} đã tự hủy do bạn không xác nhận kịp thời hạn. Tiền đã được hoàn cho khách."));
            }
        }
    }
}
