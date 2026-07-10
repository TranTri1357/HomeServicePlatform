using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Reviews.Admin.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Reviews.Admin.Queries.GetAllReviews
{
    /// <summary>Danh sách đánh giá cho Admin kiểm duyệt (lọc theo tên/điểm).</summary>
    public record GetAllReviewsQuery(string? SearchTerm, short? Rating, int PageIndex = 1, int PageSize = 10)
        : IRequest<ApiResponse<PagedResult<ReviewLookupDto>>>;
}
