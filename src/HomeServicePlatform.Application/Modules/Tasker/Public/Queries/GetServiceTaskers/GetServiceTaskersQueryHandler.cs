using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    public class GetServiceTaskersQueryHandler
        : IRequestHandler<GetServiceTaskersQuery, ApiResponse<PagedResult<ServiceTaskerSuggestionDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetServiceTaskersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<ServiceTaskerSuggestionDto>>> Handle(
            GetServiceTaskersQuery request, CancellationToken ct)
        {
            var pageIndex = PageSizeGuard.ClampIndex(request.PageIndex);
            var pageSize = PageSizeGuard.Clamp(request.PageSize, max: 50, @default: 5);
            var now = DateTimeOffset.UtcNow;

            // Cùng bộ lọc với top-5 trong GetServiceDetail: thợ đang hoạt động, hồ sơ chưa xóa,
            // có nhận đúng dịch vụ này.
            var query = _context.TaskerServices
                .AsNoTracking()
                .Where(ts => ts.ServiceId == request.ServiceId
                             && !ts.TaskerProfile.IsDeleted
                             && ts.TaskerProfile.Status == 1);

            var totalCount = await query.CountAsync(ct);

            // ⚠️ SORT PHẢI ỔN ĐỊNH cho phân trang: chỉ theo RatingAvg thì các thợ CÙNG điểm
            // (vd nhiều thợ 0★ / cùng rating) sẽ xáo trộn giữa các trang → trùng hoặc sót khi
            // "tải thêm". Thêm khóa phụ TotalReviews rồi TaskerId để thứ tự là duy nhất.
            var items = await query
                .OrderByDescending(ts => ts.TaskerProfile.RatingAvg)
                .ThenByDescending(ts => ts.TaskerProfile.TotalReviews)
                .ThenBy(ts => ts.TaskerId)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(ts => new ServiceTaskerSuggestionDto
                {
                    TaskerId = ts.TaskerId,
                    FullName = ts.TaskerProfile.User.FullName,
                    ExperienceYears = ts.TaskerProfile.ExperienceYears,
                    RatingAvg = ts.TaskerProfile.RatingAvg,
                    AvatarUrl = null,

                    // 💰 Đồng bộ với TaskerPriceQuery.IsActiveAt: giá đang hiệu lực của thợ cho dịch vụ này.
                    CurrentPrice = ts.TaskerProfile.TaskerServicePrices
                        .Where(p => p.ServiceId == request.ServiceId
                                    && p.EffectiveFrom <= now
                                    && (p.EffectiveTo == null || p.EffectiveTo > now))
                        .OrderByDescending(p => p.EffectiveFrom)
                        .Select(p => p.Price)
                        .FirstOrDefault()
                })
                .ToListAsync(ct);

            var result = new PagedResult<ServiceTaskerSuggestionDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };

            return ApiResponse<PagedResult<ServiceTaskerSuggestionDto>>.Success(
                result, "Lấy danh sách thợ theo dịch vụ thành công.");
        }
    }
}
