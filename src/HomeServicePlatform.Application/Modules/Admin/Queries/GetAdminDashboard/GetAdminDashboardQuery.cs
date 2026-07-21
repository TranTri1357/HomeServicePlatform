using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Admin.Queries.GetAdminDashboard
{
    public record GetAdminDashboardQuery : IRequest<ApiResponse<AdminDashboardDto>>;
}
