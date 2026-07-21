using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard
{
    public record GetTaskerDashboardQuery(long TaskerId) : IRequest<ApiResponse<TaskerDashboardDto>>;
}
