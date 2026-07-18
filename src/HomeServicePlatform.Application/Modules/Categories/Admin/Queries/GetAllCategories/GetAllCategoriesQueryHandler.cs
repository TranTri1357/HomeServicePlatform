using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Modules.Categories.Admin.Dtos;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Queries.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, ApiResponse<PagedResult<CategoryDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllCategoriesQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<CategoryDto>>> Handle(GetAllCategoriesQuery request, CancellationToken ct)
        {
            var query = _context.Categories.AsNoTracking().Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(c => new CategoryDto(
                    c.CategoryId,
                    c.IconUrl,
                    c.Name,
                    c.Services.Count(s => !s.IsDeleted),
                    c.Services.SelectMany(s => s.TaskerServices).Select(ts => ts.TaskerId).Distinct().Count(),
                    c.Services.SelectMany(s => s.BookingItems).Count(),
                    c.IsActive ?? false
                ))
                .ToListAsync(ct);

            var result = new PagedResult<CategoryDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<CategoryDto>>.Success(result, "Lấy danh sách thành công.");
        }
    }
}
