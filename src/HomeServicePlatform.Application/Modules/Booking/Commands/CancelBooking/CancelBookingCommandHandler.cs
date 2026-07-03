using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public CancelBookingCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Lấy thông tin đơn hàng tổng (Bookings)
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, cancellationToken);

            if (booking == null)
            {
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{request.BookingId}");
            }

            // Giả định: status = 0 là Chờ xác nhận, status = 2 hoặc 3 là Hủy đơn (Tùy thuộc business của bạn)
            // 🟢 ĐIỀU KIỆN RÀNG BUỘC CHÍ MẠNG: Đơn đã được thợ nhận (status != 0) thì không cho phép hủy tự do nữa
            if (booking.Status != BookingStatus.Pending)
            {
                throw new BadRequestException("Không thể hủy đơn hàng này do đơn đã được thợ xác nhận tiếp nhận hoặc đã hoàn thành.");
            }

            short oldStatus = (short)booking.Status;

            // 2. Cập nhật bảng đơn hàng tổng Bookings
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTimeOffset.UtcNow;

            // 3. Cập nhật tất cả các hạng mục công việc trong bảng booking_items thuộc đơn này
            var bookingItems = await _context.BookingItems
                .Where(bi => bi.BookingId == request.BookingId)
                .ToListAsync(cancellationToken);

            foreach (var item in bookingItems)
            {
                // 🟢 Ép kiểu (short) trước Enum để hết lỗi gạch đỏ
                item.Status = (short)BookingStatus.Cancelled;
                item.CancelRejectReason = request.CancelReason;
                item.UpdatedAt = DateTimeOffset.UtcNow;
            }

            // 4. Ghi nhận lịch sử thay đổi trạng thái vào bảng booking_histories (Audit Trail)
            var history = new BookingHistory
            {
                BookingId = booking.BookingId,
                OldStatus = oldStatus,
                // 🟢 Ép kiểu (short) trước Enum ở đây luôn
                NewStatus = (short)BookingStatus.Cancelled,
                ChangedBy = booking.CustomerId,
                CreatedAt = DateTimeOffset.UtcNow // 🟢 Thêm dấu phẩy vào cuối dòng này để hết lỗi cú pháp
            };
            _context.BookingHistories.Add(history);

            // 5. Lưu toàn bộ thay đổi xuống database dưới dạng một Transaction bảo toàn dữ liệu
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Hủy đơn đặt lịch thành công.");
        }
    }
}
