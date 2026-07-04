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

namespace HomeServicePlatform.Application.Modules.Disputes.Queries.GetPagedDisputes
{
    public class GetPagedDisputesQueryHandler : IRequestHandler<GetPagedDisputesQuery, ApiResponse<PagedResult<DisputeLookupDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetPagedDisputesQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<DisputeLookupDto>>> Handle(GetPagedDisputesQuery request, CancellationToken ct)
        {
            var query = from d in _context.Disputes
                        join u in _context.Users on d.RaisedById equals u.UserId
                        select new { d, u };

            if (request.Status.HasValue)
            {
                query = query.Where(x => x.d.Status == request.Status.Value);
            }

            int totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(x => x.d.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new DisputeLookupDto(
                    x.d.DisputeId,
                    x.d.BookingId,
                    x.d.RaisedById,
                    x.u.FullName,
                    x.d.Reason,
                    x.d.Status,
                    x.d.ResolutionNote,
                    x.d.RefundAmount,
                    x.d.ResolvedAt,
                    x.d.CreatedAt,
                    x.d.RowVersion
                )).ToListAsync(ct);

            var result = new PagedResult<DisputeLookupDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResult<DisputeLookupDto>>.Success(result, "Tải danh sách tranh chấp phân trang thành công.");
        }
    }
}
