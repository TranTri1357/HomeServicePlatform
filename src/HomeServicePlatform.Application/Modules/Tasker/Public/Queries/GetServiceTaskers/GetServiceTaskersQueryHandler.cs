using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using HomeServicePlatform.Application.Common.Helpers;
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

            // 📍 Lọc theo tỉnh/thành của địa chỉ khách đang đặt: chỉ giữ thợ có ÍT NHẤT một
            // địa chỉ cùng tỉnh. Dịch sang EXISTS nên phân trang vẫn đếm đúng.
            // Thợ chưa khai địa chỉ sẽ bị loại — đúng ý: không thể khẳng định họ ở gần khách.
            if (!string.IsNullOrWhiteSpace(request.ProvinceCode))
            {
                var provinceCode = request.ProvinceCode!.Trim();
                query = query.Where(ts => _context.Addresses
                    .Any(a => a.UserId == ts.TaskerId && a.ProvinceCode == provinceCode));
            }

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

            // 📍 Gắn khu vực + khoảng cách cho đúng trang vừa lấy (1 truy vấn phụ, tối đa
            // pageSize thợ). Không lồng vào Select ở trên để SQL còn dễ đọc và dễ dịch.
            var locations = await TaskerLocationResolver.LoadAsync(
                _context, items.Select(i => i.TaskerId).ToList(), ct);

            foreach (var item in items)
            {
                if (!locations.TryGetValue(item.TaskerId, out var loc)) continue;
                item.ProvinceCode = loc.ProvinceCode;
                item.DistrictCode = loc.DistrictCode;
                item.DistanceKm = TaskerLocationResolver.DistanceKm(
                    request.CustomerLat, request.CustomerLng, loc.Lat, loc.Lng);
            }

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
