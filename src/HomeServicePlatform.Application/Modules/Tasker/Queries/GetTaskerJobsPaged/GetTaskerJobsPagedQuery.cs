using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobsPaged
{
    /// <summary>
    /// Danh sách việc của thợ, gộp theo đơn + LỌC (tab trạng thái) + TÌM + PHÂN TRANG ở server.
    /// </summary>
    /// <param name="Statuses">Tập trạng thái theo tab (0 mới; 1-3 đang làm; 4-6 lịch sử). Rỗng = tất cả.</param>
    /// <param name="SearchTerm">Tìm theo mã đơn (BK123 / 123), tên khách hoặc tên dịch vụ.</param>
    public record GetTaskerJobsPagedQuery(
        long TaskerId,
        IReadOnlyList<short>? Statuses = null,
        string? SearchTerm = null,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<TaskerJobGroupDto>>>;
}
