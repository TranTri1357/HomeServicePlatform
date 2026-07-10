using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, ApiResponse<List<MyBookingDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyBookingsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<MyBookingDto>>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
        {
            // Projection trực tiếp xuống SQL: mỗi đơn kèm danh sách hạng mục (nhiều dịch vụ)
            // và các cờ trạng thái dùng để bật/tắt nút ở giao diện khách hàng.
            var result = await _context.Bookings
                .Where(b => b.CustomerId == request.CustomerId)
                .OrderByDescending(b => b.CreatedAt) // Đơn mới nhất lên đầu
                .Select(b => new MyBookingDto(
                    b.BookingId,
                    // Left join địa chỉ đơn hàng (một đơn chỉ có một địa chỉ).
                    _context.BookingAddresses
                        .Where(a => a.BookingId == b.BookingId)
                        .Select(a => a.AddressLine + ", " + a.WardCode)
                        .FirstOrDefault() ?? "Chưa cập nhật địa chỉ",
                    b.SubtotalAmount,
                    b.DiscountAmount,
                    b.FinalAmount,
                    b.Note,
                    b.CreatedAt,
                    (short)b.Status,
                    // Đã thanh toán = tồn tại giao dịch Payment ở trạng thái Paid.
                    _context.Payments.Any(p => p.BookingId == b.BookingId && p.Status == (short)PaymentStatus.Paid),
                    // Đã khiếu nại = tồn tại bản ghi Dispute cho đơn.
                    _context.Disputes.Any(d => d.BookingId == b.BookingId),
                    // Toàn bộ hạng mục của đơn (một hoặc nhiều dịch vụ).
                    _context.BookingItems
                        .Where(i => i.BookingId == b.BookingId)
                        .OrderBy(i => i.BookingItemId)
                        .Select(i => new MyBookingItemDto(
                            i.BookingItemId,
                            i.Service.Name,
                            i.TaskerId,
                            i.TaskerProfile != null && i.TaskerProfile.User != null
                                ? i.TaskerProfile.User.FullName
                                : "Đang tìm thợ...",
                            i.StartAt,
                            i.EndAt,
                            i.Quantity,
                            i.UnitPrice,
                            i.TotalPrice,
                            i.Status,
                            // Đã đánh giá = tồn tại Review chưa bị xóa cho hạng mục này.
                            _context.Reviews.Any(r => r.BookingItemId == i.BookingItemId && !r.IsDeleted)
                        ))
                        .ToList()
                ))
                .ToListAsync(cancellationToken);

            return ApiResponse<List<MyBookingDto>>.Success(result, "Lấy lịch sử đơn đặt lịch thành công.");
        }
    }
}
