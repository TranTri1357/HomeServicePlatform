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
using HomeServicePlatform.Domain.Modules.Payments.Entities;
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
                // State machine chỉ cho phép InProgress -> Completed đúng một lần,
                // nên đây là điểm ghi nhận thu nhập (đơn đã thanh toán online trước đó
                // hoặc thu tiền mặt khi hoàn thành).
                booking.CompleteWorkAndPendingPayment(request.TaskerId);

                await CreditTaskerEarningAsync(booking.BookingId, request.TaskerId, cancellationToken);

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

        /// <summary>
        /// Ghi có thu nhập (đã trừ hoa hồng) vào ví của thợ cho các hạng mục thợ đảm nhận
        /// trong đơn. Ví của thợ dùng chung bảng Wallet, khóa theo UserId — mà UserId của
        /// thợ chính là TaskerProfileId. Bản ghi WalletTransaction tham chiếu BookingId.
        /// </summary>
        private async Task CreditTaskerEarningAsync(long bookingId, long taskerId, CancellationToken ct)
        {
            // Idempotent: nếu đơn này đã ghi có thu nhập rồi thì bỏ qua (phòng khi gọi lại).
            var alreadyCredited = await _context.WalletTransactions.AnyAsync(
                t => t.ReferenceId == bookingId && t.Type == (short)WalletTransactionType.Earning, ct);
            if (alreadyCredited) return;

            var items = await _context.BookingItems
                .AsNoTracking()
                .Where(bi => bi.BookingId == bookingId && bi.TaskerId == taskerId)
                .Select(bi => new { bi.ServiceId, bi.TotalPrice })
                .ToListAsync(ct);

            if (items.Count == 0) return;

            var now = DateTimeOffset.UtcNow;

            var commissions = await _context.Commissions
                .AsNoTracking()
                .Where(c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now))
                .ToListAsync(ct);

            decimal netTotal = 0m;
            foreach (var it in items)
            {
                var rate = CommissionResolver.ResolveRate(commissions, it.ServiceId, taskerId, now);
                netTotal += CommissionResolver.NetOf(it.TotalPrice, rate);
            }

            if (netTotal <= 0m) return;

            // Ví của thợ (lazy-create nếu chưa có).
            var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == taskerId, ct);
            if (wallet == null)
            {
                wallet = new Wallet { UserId = taskerId, Balance = 0m };
                _context.Wallets.Add(wallet);
            }

            var balanceBefore = wallet.Balance;
            wallet.Balance += netTotal;

            wallet.WalletTransactions.Add(new WalletTransaction
            {
                Type = (short)WalletTransactionType.Earning,
                Amount = netTotal,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                ReferenceId = bookingId,
                CreatedAt = now
            });
        }
    }
}
