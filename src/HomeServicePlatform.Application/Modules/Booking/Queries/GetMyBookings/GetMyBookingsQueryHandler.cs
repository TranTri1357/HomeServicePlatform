using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
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
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 50 ? 50 : request.PageSize);

            var query = _context.Bookings
                .AsNoTracking()
                .Where(b => b.CustomerId == request.CustomerId);

            query = query.Where(b => !(
                b.IsEmergency
                && b.Status == BookingStatus.Cancelled
                && !b.BookingItems.Any(i => i.TaskerId != null)
                && !_context.Payments.Any(p => p.BookingId == b.BookingId
                                               && p.Status == (short)PaymentStatus.Paid)));

            if (request.Statuses is { Count: > 0 })
            {
                var statuses = request.Statuses.ToList();
                query = query.Where(b => statuses.Contains((short)b.Status));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var kw = request.SearchTerm.Trim().ToLower();
                var idPart = kw.StartsWith("bk") ? kw.Substring(2) : kw;
                long.TryParse(idPart, out var maybeId);

                query = query.Where(b =>
                    (maybeId != 0 && b.BookingId == maybeId) ||
                    b.BookingItems.Any(i => i.Service.Name.ToLower().Contains(kw)));
            }

            query = query.OrderByDescending(b => b.CreatedAt);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new MyBookingDto(
                    b.BookingId,
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
                    _context.Payments.Any(p => p.BookingId == b.BookingId && p.Status == (short)PaymentStatus.Paid),
                    _context.Disputes.Any(d => d.BookingId == b.BookingId),
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
