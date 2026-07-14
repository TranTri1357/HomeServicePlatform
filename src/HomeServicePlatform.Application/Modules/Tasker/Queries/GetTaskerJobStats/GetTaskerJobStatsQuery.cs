using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobStats
{
    /// <summary>Đếm số ĐƠN của thợ theo nhóm trạng thái (phục vụ badge + số trên mỗi tab).</summary>
    public record GetTaskerJobStatsQuery(long TaskerId) : IRequest<ApiResponse<TaskerJobStatsDto>>;

    /// <param name="Incoming">Đơn chờ thợ xác nhận (status 0).</param>
    /// <param name="Active">Đơn đang xử lý (status 1-3).</param>
    /// <param name="History">Đơn đã kết thúc (status 4-6).</param>
    public record TaskerJobStatsDto(int Incoming, int Active, int History);
}
