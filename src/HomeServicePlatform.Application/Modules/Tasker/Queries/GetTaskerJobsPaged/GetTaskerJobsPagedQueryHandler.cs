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

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobsPaged
{
    public class GetTaskerJobsPagedQueryHandler
        : IRequestHandler<GetTaskerJobsPagedQuery, ApiResponse<PagedResult<TaskerJobGroupDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerJobsPagedQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<TaskerJobGroupDto>>> Handle(GetTaskerJobsPagedQuery request, CancellationToken ct)
        {
            var pageIndex = request.PageIndex < 1 ? 1 : request.PageIndex;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 50 ? 50 : request.PageSize);
            var taskerId = request.TaskerId;

            // Chỉ đơn có hạng mục giao cho thợ này VÀ đã "chốt" (đã thanh toán, hoặc tiền mặt trả khi xong).
            var query = _context.Bookings
                .AsNoTracking()
                .Where(b => b.BookingItems.Any(i => i.TaskerId == taskerId)
                            && _context.Payments.Any(p => p.BookingId == b.BookingId
                                && (p.Status == (short)PaymentStatus.Paid
                                    || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))));

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
                    (maybeId != 0 && b.BookingId == maybeId)
                    || b.BookingItems.Any(i => i.TaskerId == taskerId && i.Service.Name.ToLower().Contains(kw))
                    || (b.Customer != null && b.Customer.FullName.ToLower().Contains(kw))
                    || _context.BookingAddresses.Any(a => a.BookingId == b.BookingId && a.FullName.ToLower().Contains(kw)));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(b => b.BookingId) // Đơn mới nhất lên đầu
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new TaskerJobGroupDto(
                    b.BookingId,
                    // Ưu tiên thông tin liên hệ khách nhập lúc đặt lịch (BookingAddress), fallback tài khoản khách.
                    _context.BookingAddresses.Where(a => a.BookingId == b.BookingId).Select(a => a.FullName).FirstOrDefault()
                        ?? (b.Customer != null ? b.Customer.FullName : "Khách hàng"),
                    _context.BookingAddresses.Where(a => a.BookingId == b.BookingId).Select(a => a.Phone).FirstOrDefault()
                        ?? (b.Customer != null ? b.Customer.Phone : ""),
                    _context.BookingAddresses.Where(a => a.BookingId == b.BookingId).Select(a => a.AddressLine).FirstOrDefault()
                        ?? "Chưa cập nhật địa chỉ",
                    (short)b.Status,
                    // Giờ bắt đầu sớm nhất + tổng tiền CHỈ tính hạng mục của thợ này trong đơn.
                    _context.BookingItems.Where(i => i.BookingId == b.BookingId && i.TaskerId == taskerId)
                        .Min(i => (System.DateTimeOffset?)i.StartAt) ?? default,
                    _context.BookingItems.Where(i => i.BookingId == b.BookingId && i.TaskerId == taskerId)
                        .Sum(i => i.TotalPrice),
                    _context.BookingItems.Where(i => i.BookingId == b.BookingId && i.TaskerId == taskerId)
                        .OrderBy(i => i.StartAt)
                        .Select(i => new TaskerJobItemDto(
                            i.BookingItemId,
                            i.Service.Name,
                            i.StartAt,
                            i.EndAt,
                            i.TotalPrice,
                            i.Status))
                        .ToList()
                ))
                .ToListAsync(ct);

            var result = new PagedResult<TaskerJobGroupDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<TaskerJobGroupDto>>.Success(result, "Lấy danh sách công việc của thợ thành công.");
        }
    }
}
