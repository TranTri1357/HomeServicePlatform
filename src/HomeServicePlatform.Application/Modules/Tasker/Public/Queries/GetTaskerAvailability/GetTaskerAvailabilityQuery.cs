using System;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability
{
    public record GetTaskerAvailabilityQuery(long TaskerId, DateOnly Date, double? Lat = null, double? Lng = null)
        : IRequest<ApiResponse<TaskerAvailabilityDto>>;
}
