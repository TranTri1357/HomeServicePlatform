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
                // State machine chỉ cho phép InProgress -> Completed đúng một lần,
                // nên đây là điểm ghi nhận thu nhập (đơn đã thanh toán online trước đó
                // hoặc thu tiền mặt khi hoàn thành).
                booking.CompleteWorkAndPendingPayment(request.TaskerId);

                await SettleBookingFundsAsync(booking.BookingId, request.TaskerId, cancellationToken);

                // 📈 Tăng độ tin cậy: đơn hoàn thành cộng vào CompletedCount của thợ.
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

        /// <summary>
        /// TẤT TOÁN đơn: giải phóng khoản sàn đang giữ hộ trong ví ký quỹ và chia làm hai đường —
        /// thu nhập thực nhận về ví thợ, hoa hồng về ví doanh thu của sàn.
        ///
        /// Bút toán (tổng bằng 0):
        ///     ví ký quỹ  − heldAmount
        ///     ví thợ     + netTotal
        ///     ví doanh thu + commission        (netTotal + commission = heldAmount)
        ///
        /// Ví của thợ dùng chung bảng Wallet, khóa theo UserId — mà UserId của thợ chính là
        /// TaskerProfileId. Bản ghi WalletTransaction tham chiếu BookingId.
        /// </summary>
        private async Task SettleBookingFundsAsync(long bookingId, long taskerId, CancellationToken ct)
        {
            // Idempotent: khóa theo việc ví ký quỹ đã nhả tiền của đơn này hay chưa. Dùng EscrowOut
            // (thay vì Earning) vì đó là vế LUÔN xuất hiện khi đơn được định đoạt — kể cả trường hợp
            // thu nhập thực nhận bằng 0 (hoa hồng ăn hết) hay đơn đã được hoàn tiền trước đó.
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

            // Số tiền HỆ THỐNG thực sự giữ cho đơn này = tổng các khoản đã thanh toán thành
            // công (vd: tiền cọc 30%, hoặc trả hết). Phần chưa thu qua hệ thống, thợ đã/đang
            // nhận tiền mặt trực tiếp từ khách nên KHÔNG cộng lại vào ví (tránh tính 2 lần).
            decimal heldAmount = await _context.Payments
                .Where(p => p.BookingId == bookingId && p.Status == (short)PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            // Đơn thuần tiền mặt: sàn không giữ đồng nào nên không có gì để giải ngân.
            if (heldAmount <= 0m) return;

            var commissions = await _context.Commissions
                .AsNoTracking()
                .Where(c => c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now))
                .ToListAsync(ct);

            // Hoa hồng của sàn tính trên TỔNG giá đơn (gross từng hạng mục).
            decimal commissionTotal = 0m;
            foreach (var it in items)
            {
                var rate = CommissionResolver.ResolveRate(commissions, it.ServiceId, taskerId, now);
                commissionTotal += CommissionResolver.CommissionOf(it.TotalPrice, rate);
            }

            // Không thể khấu hoa hồng nhiều hơn số đang giữ (vd đơn trả cọc một phần, hoa hồng tính
            // trên giá trị đơn đầy đủ). Chặn tại đây để hai vế luôn khớp và ví ký quỹ không bị âm.
            var commissionDue = Math.Round(Math.Min(commissionTotal, heldAmount), 2, MidpointRounding.AwayFromZero);
            var netTotal = heldAmount - commissionDue;

            // Nạp ba ví liên quan trong MỘT truy vấn.
            var wallets = await WalletLedger.ResolveAsync(
                _context,
                new[] { taskerId, SystemAccounts.EscrowUserId, SystemAccounts.RevenueUserId },
                ct);

            // Vế ghi NỢ — ví ký quỹ nhả toàn bộ khoản đang giữ của đơn.
            WalletLedger.Debit(
                wallets[SystemAccounts.EscrowUserId],
                WalletTransactionType.EscrowOut,
                heldAmount,
                bookingId,
                now,
                note: $"Tất toán đơn BK{bookingId}",
                insufficientMessage: $"Ví ký quỹ không đủ số dư để tất toán đơn BK{bookingId}.");

            // Vế ghi CÓ (1) — thu nhập thực nhận của thợ.
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

            // Vế ghi CÓ (2) — hoa hồng về ví doanh thu (trước đây khoản này chỉ được tính rồi bỏ đi).
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
