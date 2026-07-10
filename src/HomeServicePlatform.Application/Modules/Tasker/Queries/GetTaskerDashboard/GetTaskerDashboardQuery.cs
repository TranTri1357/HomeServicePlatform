using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard
{
    /// <summary>Thống kê trang chủ của thợ đang đăng nhập. TaskerId (= UserId) từ Token.</summary>
    public record GetTaskerDashboardQuery(long TaskerId) : IRequest<ApiResponse<TaskerDashboardDto>>;
}
