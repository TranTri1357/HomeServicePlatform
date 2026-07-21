using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    public record GetServiceTaskerCardQuery(
        long ServiceId,
        long TaskerId,
        double? CustomerLat = null,
        double? CustomerLng = null)
        : IRequest<ApiResponse<ServiceTaskerSuggestionDto?>>;
}
