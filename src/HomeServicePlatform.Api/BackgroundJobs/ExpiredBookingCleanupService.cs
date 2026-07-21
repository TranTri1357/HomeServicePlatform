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

        private enum CancelReason
        {
            HoldExpired,
            EmergencyNoTasker,
            NotConfirmedPaid
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
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
            var expiredThreshold = BookingSlotOccupancy.FreshHoldSince(nowUtc);

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

            var expiredEmergencies = await context.Bookings
                .Include(b => b.BookingItems)
                .Where(b => b.Status == BookingStatus.Pending
                            && b.IsEmergency
                            && b.EmergencyExpiresAt != null
                            && b.EmergencyExpiresAt < nowUtc)
                .ToListAsync(ct);

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
                await context.AcquireBookingClaimLockAsync(booking.BookingId, ct);

                var currentStatus = await context.Bookings.AsNoTracking()
                    .Where(b => b.BookingId == booking.BookingId)
                    .Select(b => (short)b.Status)
                    .FirstOrDefaultAsync(ct);
                if (currentStatus != (short)BookingStatus.Pending)
                {
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
                var body = reason == CancelReason.EmergencyNoTasker
                    ? $"Đơn khẩn cấp {code} đã hết hạn vì chưa có thợ nào nhận. Bạn có thể thử gọi lại."
                    : $"Đơn {code} đã tự hủy do quá hạn giữ chỗ mà chưa hoàn tất thanh toán.";

                context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId, NotificationType.BookingAutoCancelled, "Đơn đã tự hủy", body));
                return;
            }

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
