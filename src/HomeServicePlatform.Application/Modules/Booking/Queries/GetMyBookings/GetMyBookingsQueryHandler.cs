using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, ApiResponse<PagedResult<MyBookingDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyBookingsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<MyBookingDto>>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
        {
            // Chuẩn hoá tham số phân trang (chặn giá trị vô lý).
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 50 ? 50 : request.PageSize);

            var query = _context.Bookings
                .AsNoTracking()
                .Where(b => b.CustomerId == request.CustomerId);

            // Lọc theo tab (tập trạng thái) — thực hiện tại SQL, không lọc ở client nữa.
            if (request.Statuses is { Count: > 0 })
            {
                var statuses = request.Statuses.ToList();
                query = query.Where(b => statuses.Contains((short)b.Status));
            }

            // Tìm theo mã đơn (BK123 / 123) HOẶC tên dịch vụ trong đơn (dùng trigram index).
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var kw = request.SearchTerm.Trim().ToLower();
                var idPart = kw.StartsWith("bk") ? kw.Substring(2) : kw;
                long.TryParse(idPart, out var maybeId);

                query = query.Where(b =>
                    (maybeId != 0 && b.BookingId == maybeId) ||
                    b.BookingItems.Any(i => i.Service.Name.ToLower().Contains(kw)));
            }

            query = query.OrderByDescending(b => b.CreatedAt); // Đơn mới nhất lên đầu (khớp index customer_id, created_at)

            var totalCount = await query.CountAsync(cancellationToken);

            // Projection trực tiếp xuống SQL: mỗi đơn kèm danh sách hạng mục (nhiều dịch vụ)
            // và các cờ trạng thái dùng để bật/tắt nút ở giao diện khách hàng.
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
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

            var result = new PagedResult<MyBookingDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<MyBookingDto>>.Success(result, "Lấy lịch sử đơn đặt lịch thành công.");
        }
    }
}
