using System;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability
{
    /// <summary>Khung giờ trống của một thợ trong ngày (public, cho khách đặt lịch).</summary>
    public record GetTaskerAvailabilityQuery(long TaskerId, DateOnly Date)
        : IRequest<ApiResponse<TaskerAvailabilityDto>>;
}
