using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings
{
    public class GetAllBookingsQueryHandler : IRequestHandler<GetPagedBookingsQuery, ApiResponse<PagedResult<BookingLookupDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllBookingsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<BookingLookupDto>>> Handle(GetPagedBookingsQuery request, CancellationToken ct)
        {
            // Join các bảng để kéo dữ liệu Tên khách hàng và Địa chỉ thi công ra Flat-DTO nhanh nhất
            var query = from b in _context.Bookings.AsNoTracking()
                        join u in _context.Users on b.CustomerId equals u.UserId
                        join a in _context.BookingAddresses on b.BookingId equals a.BookingId
                        select new { b, u, a };

            // Tìm kiếm động
            if (request.CustomerId.HasValue)
                query = query.Where(x => x.b.CustomerId == request.CustomerId.Value);

            if (request.Status.HasValue)
                query = query.Where(x => x.b.Status == (BookingStatus)request.Status.Value);

            if (request.FromDate.HasValue)
                query = query.Where(x => x.b.CreatedAt >= request.FromDate.Value);

            if (request.ToDate.HasValue)
                query = query.Where(x => x.b.CreatedAt <= request.ToDate.Value);

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.b.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(x => new BookingLookupDto(
                    x.b.BookingId,
                    x.b.CustomerId,
                    x.u.FullName,
                    x.b.Status,
                    x.b.FinalAmount,
                    x.a.AddressLine,
                    x.b.CreatedAt,
                    x.b.RowVersion
                )).ToListAsync(ct);

            var result = new PagedResult<BookingLookupDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<BookingLookupDto>>.Success(result, "Tải danh sách đơn đặt lịch thành công.");
        }
    }
}
