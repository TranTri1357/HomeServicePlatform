using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Admin.Queries.GetAdminDashboard
{
    /// <summary>Số liệu tổng quan cho trang Dashboard của Admin.</summary>
    public record GetAdminDashboardQuery : IRequest<ApiResponse<AdminDashboardDto>>;
}
