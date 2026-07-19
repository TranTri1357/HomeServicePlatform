using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetPagedPayments
{
    public class GetPagedPaymentsQueryHandler : IRequestHandler<GetPagedPaymentsQuery, ApiResponse<PagedResult<PaymentLookupDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetPagedPaymentsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<PaymentLookupDto>>> Handle(GetPagedPaymentsQuery request, CancellationToken ct)
        {
            var query = _context.Payments.AsNoTracking();

            // 1. Áp dụng các bộ lọc động
            if (request.Status.HasValue)
            {
                query = query.Where(x => x.Status == request.Status.Value);
            }

            if (request.Method.HasValue)
            {
                query = query.Where(x => x.Method == request.Method.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.TransactionCode))
            {
                string code = request.TransactionCode.Trim();
                // TransactionCode nullable: đơn tiền mặt/đơn chưa qua cổng chưa có mã.
                // Thiếu vế kiểm null thì lọc theo mã sẽ ném NullReferenceException -> trang
                // quản lý thanh toán trả 500 ngay khi admin gõ vào ô tìm kiếm.
                query = query.Where(x => x.TransactionCode != null && x.TransactionCode.Contains(code));
            }

            // 2. Tính tổng số lượng dòng
            int totalCount = await query.CountAsync(ct);

            // 3. Phân trang và map về Flat DTO
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(x => new PaymentLookupDto(
                    x.PaymentId,
                    x.BookingId,
                    x.Amount,
                    x.Method,
                    x.Status,
                    x.TransactionCode,
                    x.PaidAt,
                    x.CreatedAt
                )).ToListAsync(ct);

            var result = new PagedResult<PaymentLookupDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<PaymentLookupDto>>.Success(result, "Tải danh sách thanh toán thành công.");
        }
    }
}
