using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerServiceOptions
{
    /// <summary>Danh sách dịch vụ + giá của một thợ (public, cho khách chọn khi đặt lịch).</summary>
    public record GetTaskerServiceOptionsQuery(long TaskerId)
        : IRequest<ApiResponse<List<TaskerServiceOptionDto>>>;
}
