using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobStats
{
    public record GetTaskerJobStatsQuery(long TaskerId) : IRequest<ApiResponse<TaskerJobStatsDto>>;

    public record TaskerJobStatsDto(int Incoming, int Active, int History);
}
