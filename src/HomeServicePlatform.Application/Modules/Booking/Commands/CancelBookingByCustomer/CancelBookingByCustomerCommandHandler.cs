using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBookingByCustomer
{
    public class CancelBookingByCustomerCommandHandler
        : IRequestHandler<CancelBookingByCustomerCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public CancelBookingByCustomerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CancelBookingByCustomerCommand request, CancellationToken cancellationToken)
        {
            // 1. Lấy đơn hàng tổng (Bookings)
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{request.BookingId}");
            }

            // 2. 🔒 CHỐNG TRUY CẬP TRÁI PHÉP: Chỉ chủ đơn mới được hủy đơn của chính mình.
            if (booking.CustomerId != request.CustomerId)
            {
                throw new ForbiddenException("Bạn không có quyền hủy đơn đặt lịch này.");
            }

            // 3. Chỉ cho phép hủy khi đơn còn Chờ xác nhận (chưa có thợ tiếp nhận).
            if (booking.Status != BookingStatus.Pending)
            {
                throw new BadRequestException("Không thể hủy đơn hàng này do đơn đã được thợ tiếp nhận hoặc đã kết thúc.");
            }

            short oldStatus = (short)booking.Status;

            // 4. Cập nhật đơn hàng tổng
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTimeOffset.UtcNow;

            // 5. Cập nhật tất cả hạng mục trong booking_items thuộc đơn này
            var bookingItems = await _context.BookingItems
                .Where(bi => bi.BookingId == request.BookingId)
                .ToListAsync(cancellationToken);

            foreach (var item in bookingItems)
            {
                item.Status = (short)BookingStatus.Cancelled;
                item.CancelRejectReason = request.CancelReason;
                item.UpdatedAt = DateTimeOffset.UtcNow;
            }

            // 6. Ghi nhận lịch sử thay đổi trạng thái (Audit Trail)
            var history = new BookingHistory
            {
                BookingId = booking.BookingId,
                OldStatus = oldStatus,
                NewStatus = (short)BookingStatus.Cancelled,
                ChangedBy = request.CustomerId,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _context.BookingHistories.Add(history);

            // 7. Lưu toàn bộ thay đổi trong một Transaction
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Hủy đơn đặt lịch thành công.");
        }
    }
}
