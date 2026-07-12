using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServicesExplorer
{
    public class GetServicesExplorerQueryHandler : IRequestHandler<GetServicesExplorerQuery, ApiResponse<PagedResult<ServiceExplorerDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetServicesExplorerQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<ServiceExplorerDto>>> Handle(GetServicesExplorerQuery request, CancellationToken ct)
        {
            var query = _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDeleted);

            if (request.CategoryId.HasValue)
                query = query.Where(s => s.CategoryId == request.CategoryId);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var keyword = request.SearchTerm.ToLower();
                query = query.Where(s => s.Name.ToLower().Contains(keyword));
            }

            var projectedQuery = query.Select(s => new ServiceExplorerDto
            {
                ServiceId = s.ServiceId,
                Name = s.Name,
                Description = s.Description,
                DurationMinutes = s.DurationMinutes,
                TotalBookings = s.BookingItems.Count(),
                StartingPrice = s.TaskerServicePrices
                    .Where(p => p.EffectiveTo == null || p.EffectiveTo > DateTimeOffset.UtcNow)
                    .Min(p => (decimal?)p.Price) ?? 0,
                // Phương án (a): TB rating của các thợ cung cấp dịch vụ này, chỉ tính thợ
                // ĐÃ có đánh giá (TotalReviews > 0) để RatingAvg=0 (chưa có) không kéo điểm.
                AvgRating = s.TaskerServices
                    .Where(ts => !ts.TaskerProfile.IsDeleted && ts.TaskerProfile.TotalReviews > 0)
                    .Select(ts => (decimal?)ts.TaskerProfile.RatingAvg)
                    .Average() ?? 0,
                ImageUrl = null // Gắn URL ảnh mặc định ở đây nếu muốn
            });

            if (request.MinPrice.HasValue)
                projectedQuery = projectedQuery.Where(s => s.StartingPrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                projectedQuery = projectedQuery.Where(s => s.StartingPrice <= request.MaxPrice.Value);

            if (request.MinRating.HasValue)
                projectedQuery = projectedQuery.Where(s => s.AvgRating >= request.MinRating.Value);

            projectedQuery = request.SortBy?.ToLower() switch
            {
                "price_asc" => projectedQuery.OrderBy(s => s.StartingPrice),
                "price_desc" => projectedQuery.OrderByDescending(s => s.StartingPrice),
                "rating" => projectedQuery.OrderByDescending(s => s.AvgRating),
                "popular" => projectedQuery.OrderByDescending(s => s.TotalBookings),
                _ => projectedQuery.OrderByDescending(s => s.TotalBookings)
            };

            var totalCount = await projectedQuery.CountAsync(ct);
            var items = await projectedQuery
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(ct);

            var result = new PagedResult<ServiceExplorerDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResult<ServiceExplorerDto>>.Success(result, "Lấy danh sách dịch vụ thành công");
        }
    }
}
