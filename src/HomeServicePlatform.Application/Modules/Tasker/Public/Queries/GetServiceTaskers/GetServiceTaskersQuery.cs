using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    public record GetServiceTaskersQuery(
        long ServiceId,
        int PageIndex = 1,
        int PageSize = 5,
        string? ProvinceCode = null,
        double? CustomerLat = null,
        double? CustomerLng = null)
        : IRequest<ApiResponse<PagedResult<ServiceTaskerSuggestionDto>>>;
}
