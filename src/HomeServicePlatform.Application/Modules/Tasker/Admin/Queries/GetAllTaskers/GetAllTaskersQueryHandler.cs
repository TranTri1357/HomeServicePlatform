using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetAllTaskers
{
    public class GetAllTaskersQueryHandler : IRequestHandler<GetAllTaskersQuery, ApiResponse<PagedResult<TaskerDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllTaskersQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<TaskerDto>>> Handle(GetAllTaskersQuery request, CancellationToken ct)
        {
            var query = _context.TaskerProfiles
                .AsNoTracking()
                .Where(t => !t.IsDeleted);

            // Lọc theo trạng thái (Ví dụ: 0: Chờ duyệt, 1: Hoạt động, 2: Bị khóa)
            if (request.Status.HasValue)
                query = query.Where(t => t.Status == request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(t =>
                    t.User.FullName.ToLower().Contains(search) ||
                    t.User.Phone.Contains(search));
            }

            var totalCount = await query.CountAsync(ct);

            var coreTaskers = await query
                .OrderByDescending(t => t.User.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(t => new {
                    t.TaskerProfileId,
                    t.User.FullName,
                    t.User.Phone,
                    t.RatingAvg,
                    TotalJobs = _context.BookingItems.Count(bi => bi.TaskerId == t.TaskerProfileId),
                    t.Status,
                    t.User.CreatedAt
                })
                .ToListAsync(ct);

            var taskerIds = coreTaskers.Select(t => t.TaskerProfileId).ToList();

            var skillsData = await _context.TaskerServices
                .AsNoTracking()
                .Where(ts => taskerIds.Contains(ts.TaskerId))
                .Select(ts => new { ts.TaskerId, ServiceName = ts.Service.Name })
                .ToListAsync(ct);

            var skillsLookup = skillsData
                .GroupBy(s => s.TaskerId)
                .ToDictionary(g => g.Key, g => g.Select(s => s.ServiceName).ToList());

            var items = coreTaskers.Select(t => new TaskerDto(
                t.TaskerProfileId,
                t.FullName,
                t.Phone,
                skillsLookup.ContainsKey(t.TaskerProfileId) ? skillsLookup[t.TaskerProfileId] : new List<string>(),
                t.RatingAvg,
                t.TotalJobs,
                t.Status,
                t.CreatedAt
            )).ToList();

            var result = new PagedResult<TaskerDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<TaskerDto>>.Success(result, "Lấy danh sách thợ thành công.");
        }
    }
}
