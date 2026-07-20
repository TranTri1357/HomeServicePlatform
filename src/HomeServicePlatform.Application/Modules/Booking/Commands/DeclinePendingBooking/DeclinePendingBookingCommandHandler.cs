using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclinePendingBooking
{
    /// <summary>
    /// Thợ từ chối đơn thường chưa nhận: đơn chuyển sang Cancelled và khách được hoàn 100%.
    ///
    /// <para>Trước đây nút "Từ chối" trên app thợ gọi thẳng <c>/cancel</c>, mà handler đó chỉ chấp
    /// nhận Accepted/OnTheWay/InProgress — nên mọi lần bấm đều 400. Thợ không có cách nào thoát khỏi
    /// đơn chưa nhận, và tiền khách nằm chờ tới khi job dọn đơn quá hạn chạy.</para>
    /// </summary>
    public class DeclinePendingBookingCommandHandler
        : IRequestHandler<DeclinePendingBookingCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;

        public DeclinePendingBookingCommandHandler(IApplicationDbContext context, IUnitOfWork unitOfWork)
        {
            _context = context;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeclinePendingBookingCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{request.BookingId}");

            // 🔒 Chỉ thợ ĐƯỢC GÁN mới được từ chối đơn đó.
            if (!booking.BookingItems.Any(bi => bi.TaskerId == request.TaskerId))
                throw new ForbiddenException("Bạn không phụ trách đơn này nên không thể từ chối.");

            // 🚨 Đơn khẩn cấp là broadcast nhiều thợ, có luồng bỏ qua riêng (DeclineEmergencyBooking)
            // vốn KHÔNG hủy đơn — đơn vẫn treo cho thợ khác nhận. Đừng để nhầm sang đây mà hủy oan.
            if (booking.IsEmergency)
                throw new BadRequestException("Đơn khẩn cấp dùng chức năng bỏ qua riêng, không từ chối ở đây.");

            if (booking.Status != BookingStatus.Pending)
                throw new BadRequestException("Chỉ từ chối được đơn chưa nhận. Đơn đã nhận thì dùng chức năng hủy.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // 🔒 Khóa theo đơn rồi ĐỌC LẠI: thợ có thể vừa bấm nhận ở tab khác, hoặc job dọn đơn
                // quá hạn đang xử lý đúng đơn này. Không có bước này thì đơn có thể vừa Accepted vừa
                // bị hủy, hoặc hoàn tiền hai lần.
                await _context.AcquireBookingClaimLockAsync(booking.BookingId, ct);

                var currentStatus = await _context.Bookings.AsNoTracking()
                    .Where(b => b.BookingId == booking.BookingId)
                    .Select(b => (short)b.Status)
                    .FirstOrDefaultAsync(ct);
                // Ném thẳng — khối catch bên dưới lo rollback (rollback hai lần sẽ tự nó ném lỗi khác).
                if (currentStatus != (short)BookingStatus.Pending)
                    throw new BadRequestException("Đơn đã đổi trạng thái, không từ chối được nữa.");

                var now = DateTimeOffset.UtcNow;
                var code = $"BK{booking.BookingId}";

                short oldStatus = (short)booking.Status;
                booking.Status = BookingStatus.Cancelled;
                booking.UpdatedAt = now;
                foreach (var item in booking.BookingItems)
                {
                    item.Status = (short)BookingStatus.Cancelled;
                    item.CancelRejectReason = request.DeclineReason;
                    item.UpdatedAt = now;
                }

                _context.BookingHistories.Add(new BookingHistory
                {
                    BookingId = booking.BookingId,
                    OldStatus = oldStatus,
                    NewStatus = (short)BookingStatus.Cancelled,
                    ChangedBy = request.TaskerId,
                    CreatedAt = now
                });

                // 💸 Hoàn 100%: khách đã trả tiền trước khi thợ kịp xem đơn, việc thợ không nhận
                // hoàn toàn không phải lỗi của khách — không áp phí hủy, sàn không giữ lại gì.
                var totalPaid = await _context.Payments
                    .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                    .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

                var outcome = await RefundExecutor.IssueRefundAsync(
                    _context, booking,
                    refundAmount: totalPaid, penaltyAmount: 0m,
                    RefundInitiator.Tasker,
                    $"Thợ từ chối đơn {code}: {request.DeclineReason}",
                    now, ct);

                // 🔔 Báo khách. Dùng type riêng để không lẫn với "thợ hủy đơn đã nhận" — hai việc
                // khác hẳn nhau về mức độ nghiêm trọng.
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.BookingDeclinedByTasker,
                    "Thợ chưa thể nhận đơn",
                    $"Đơn {code} chưa được thợ tiếp nhận. Lý do: {request.DeclineReason}. " +
                    "Bạn có thể đặt lại với thợ khác."));

                if (outcome.Executed && outcome.RefundedToCustomer > 0)
                {
                    _context.Notifications.Add(NotificationBuilder.Build(
                        booking.CustomerId,
                        NotificationType.RefundIssued,
                        "Đã hoàn tiền",
                        $"Đơn {code} được hoàn 100% ({outcome.RefundedToCustomer:#,##0}đ) vào ví của bạn."));
                }

                // ⚠️ CỐ Ý KHÔNG gọi PenalizeTaskerAsync như luồng hủy đơn đã nhận: từ chối đơn chưa
                // nhận không phải là bỏ việc giữa chừng, phạt ở đây sẽ đẩy thợ tới ngưỡng khóa tài
                // khoản một cách oan uổng.

                await _context.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync();

                return ApiResponse<bool>.Success(true,
                    outcome.RefundedToCustomer > 0
                        ? $"Đã từ chối đơn và hoàn {outcome.RefundedToCustomer:#,##0}đ cho khách."
                        : "Đã từ chối đơn.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
