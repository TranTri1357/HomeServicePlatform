using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions
{
    public class GetAllCommissionsQueryHandler
        : IRequestHandler<GetAllCommissionsQuery, ApiResponse<PagedResult<CommissionDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllCommissionsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<CommissionDto>>> Handle(GetAllCommissionsQuery request, CancellationToken ct)
        {
            var query = _context.Commissions.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    (c.Service != null && c.Service.Name.ToLower().Contains(search)) ||
                    (c.TaskerProfile != null && c.TaskerProfile.User.FullName.ToLower().Contains(search)));
            }

            var now = DateTimeOffset.UtcNow;
            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(c => new CommissionDto(
                    c.CommissionId,
                    c.ServiceId,
                    c.Service != null ? c.Service.Name : null,
                    c.TaskerId,
                    c.TaskerProfile != null ? c.TaskerProfile.User.FullName : null,
                    c.CommissionRate,
                    c.EffectiveFrom,
                    c.EffectiveTo,
                    c.EffectiveTo == null || c.EffectiveTo > now))
                .ToListAsync(ct);

            var result = new PagedResult<CommissionDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResult<CommissionDto>>.Success(result, "Lấy danh sách hoa hồng thành công.");
        }
    }
}
