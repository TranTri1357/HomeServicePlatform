using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome
{
    public record GetTaskerIncomeQuery(long TaskerId, int Page = 1, int PageSize = 20)
        : IRequest<ApiResponse<TaskerIncomeDto>>;
}
