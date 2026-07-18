using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Reviews.Admin.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Reviews.Admin.Queries.GetAllReviews
{
    public class GetAllReviewsQueryHandler
        : IRequestHandler<GetAllReviewsQuery, ApiResponse<PagedResult<ReviewLookupDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllReviewsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<ReviewLookupDto>>> Handle(GetAllReviewsQuery request, CancellationToken ct)
        {
            var query = _context.Reviews
                .AsNoTracking()
                .Where(r => !r.IsDeleted);

            if (request.Rating.HasValue)
                query = query.Where(r => r.Rating == request.Rating.Value);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Customer.FullName.ToLower().Contains(search) ||
                    r.TaskerProfile.User.FullName.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(r => new ReviewLookupDto(
                    r.ReviewId,
                    r.Customer.FullName,
                    r.TaskerProfile.User.FullName,
                    r.BookingItem.Service.Name,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt))
                .ToListAsync(ct);

            var result = new PagedResult<ReviewLookupDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<ReviewLookupDto>>.Success(result, "Lấy danh sách đánh giá thành công.");
        }
    }
}
