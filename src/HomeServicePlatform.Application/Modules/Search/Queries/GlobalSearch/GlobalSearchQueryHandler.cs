using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Search.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Search.Queries.GlobalSearch
{
    public class GlobalSearchQueryHandler : IRequestHandler<GlobalSearchQuery, ApiResponse<SearchResultDto>>
    {
        private readonly IApplicationDbContext _context;

        public GlobalSearchQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<SearchResultDto>> Handle(GlobalSearchQuery request, CancellationToken ct)
        {

            var keyword = request.Keyword.ToLower();

            var categories = await _context.Categories
                .Where(x => x.Name.ToLower().Contains(keyword) && !x.IsDeleted && x.IsActive == true)
                .OrderBy(x => x.Name)
                .Select(x => new CategoryResult(x.CategoryId, x.Name, x.IconUrl ?? ""))
                .Take(5).ToListAsync(ct);

            var services = await _context.Services
                .Where(x => x.Name.ToLower().Contains(keyword) && x.IsActive && !x.IsDeleted)
                .OrderBy(x => x.Name)
                .Select(x => new ServiceResult(x.ServiceId, x.Name))
                .Take(5).ToListAsync(ct);

            var taskers = await _context.TaskerProfiles
                .AsNoTracking()
                .Where(x => x.User.FullName.ToLower().Contains(keyword) && !x.IsDeleted)
                .OrderByDescending(x => x.RatingAvg)
                .Select(x => new TaskerResult(x.TaskerProfileId, x.User.FullName, x.RatingAvg, x.TotalReviews))
                .Take(5).ToListAsync(ct);

            var result = new SearchResultDto { Categories = categories, Services = services, Taskers = taskers };

            return ApiResponse<SearchResultDto>.Success(result, "Tìm kiếm thành công");
        }
    }
}
