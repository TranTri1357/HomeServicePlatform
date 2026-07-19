using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    /// <summary>
    /// Danh sách thợ nhận một dịch vụ, SẮP THEO ĐÁNH GIÁ + PHÂN TRANG "tải thêm".
    /// Tách khỏi <c>GetServiceDetail</c> (vốn chỉ trả top 5 để xem nhanh) để khách xem
    /// được toàn bộ thợ khi bấm "Xem thêm".
    /// </summary>
    public record GetServiceTaskersQuery(long ServiceId, int PageIndex = 1, int PageSize = 5)
        : IRequest<ApiResponse<PagedResult<ServiceTaskerSuggestionDto>>>;
}
