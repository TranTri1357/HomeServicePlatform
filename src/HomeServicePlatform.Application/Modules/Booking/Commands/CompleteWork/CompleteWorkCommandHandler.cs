using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork
{
    public class CompleteWorkCommandHandler : IRequestHandler<CompleteWorkCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;

        public CompleteWorkCommandHandler(IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CompleteWorkCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            try
            {
                booking.CompleteWorkAndPendingPayment(request.TaskerId);

                await SettleBookingFundsAsync(booking.BookingId, request.TaskerId, cancellationToken);

                var profile = await _context.TaskerProfiles
                    .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId, cancellationToken);
                profile?.RecordCompletion();

                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.WorkCompleted,
                    "Hoàn thành công việc",
                    $"Đơn BK{booking.BookingId} đã hoàn thành. Vui lòng thanh toán và đánh giá thợ."));

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Đã gửi hóa đơn dịch vụ, hệ thống chuyển sang trạng thái chờ thanh toán và hoàn thành.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }

        private async Task SettleBookingFundsAsync(long bookingId, long taskerId, CancellationToken ct)
        {
            var alreadySettled = await _context.WalletTransactions.AnyAsync(
                t => t.ReferenceId == bookingId && t.Type == (short)WalletTransactionType.EscrowOut, ct);
            if (alreadySettled) return;

            var items = await _context.BookingItems
                .AsNoTracking()
                .Where(bi => bi.BookingId == bookingId && bi.TaskerId == taskerId)
                .Select(bi => new { bi.ServiceId, bi.TotalPrice })
                .ToListAsync(ct);

            if (items.Count == 0) return;

            var now = DateTimeOffset.UtcNow;

            decimal heldAmount = await _context.Payments
                .Where(p => p.BookingId == bookingId && p.Status == (short)PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            if (heldAmount <= 0m) return;

            var commissions = await _context.Commissions
                .AsNoTracking()
                .Where(c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now))
                .ToListAsync(ct);

            decimal commissionTotal = 0m;
            foreach (var it in items)
            {
                var rate = CommissionResolver.ResolveRate(commissions, it.ServiceId, taskerId, now);
                commissionTotal += CommissionResolver.CommissionOf(it.TotalPrice, rate);
            }

            var commissionDue = Math.Round(Math.Min(commissionTotal, heldAmount), 2, MidpointRounding.AwayFromZero);
            var netTotal = heldAmount - commissionDue;

            var wallets = await WalletLedger.ResolveAsync(
                _context,
                new[] { taskerId, SystemAccounts.EscrowUserId, SystemAccounts.RevenueUserId },
                ct);

            WalletLedger.Debit(
                wallets[SystemAccounts.EscrowUserId],
                WalletTransactionType.EscrowOut,
                heldAmount,
                bookingId,
                now,
                note: $"Tất toán đơn BK{bookingId}",
                insufficientMessage: $"Ví ký quỹ không đủ số dư để tất toán đơn BK{bookingId}.");

            if (netTotal > 0m)
            {
                WalletLedger.Credit(
                    wallets[taskerId],
                    WalletTransactionType.Earning,
                    netTotal,
                    bookingId,
                    now,
                    note: $"Thu nhập đơn BK{bookingId}");
            }

            if (commissionDue > 0m)
            {
                WalletLedger.Credit(
                    wallets[SystemAccounts.RevenueUserId],
                    WalletTransactionType.Commission,
                    commissionDue,
                    bookingId,
                    now,
                    note: $"Hoa hồng đơn BK{bookingId}");
            }
        }
    }
}
