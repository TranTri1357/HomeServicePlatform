using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetServiceTaskers
{
    public class GetServiceTaskerCardQueryHandler
        : IRequestHandler<GetServiceTaskerCardQuery, ApiResponse<ServiceTaskerSuggestionDto?>>
    {
        private readonly IApplicationDbContext _context;

        public GetServiceTaskerCardQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ServiceTaskerSuggestionDto?>> Handle(
            GetServiceTaskerCardQuery request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var card = await _context.TaskerServices
                .AsNoTracking()
                .Where(ts => ts.ServiceId == request.ServiceId
                             && ts.TaskerId == request.TaskerId
                             && !ts.TaskerProfile.IsDeleted
                             && ts.TaskerProfile.Status == 1)
                .Select(ts => new ServiceTaskerSuggestionDto
                {
                    TaskerId = ts.TaskerId,
                    FullName = ts.TaskerProfile.User.FullName,
                    ExperienceYears = ts.TaskerProfile.ExperienceYears,
                    RatingAvg = ts.TaskerProfile.RatingAvg,
                    AvatarUrl = null,
                    CurrentPrice = ts.TaskerProfile.TaskerServicePrices
                        .Where(p => p.ServiceId == request.ServiceId
                                    && p.EffectiveFrom <= now
                                    && (p.EffectiveTo == null || p.EffectiveTo > now))
                        .OrderByDescending(p => p.EffectiveFrom)
                        .Select(p => p.Price)
                        .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);

            if (card != null)
            {
                var locations = await TaskerLocationResolver.LoadAsync(
                    _context, new[] { card.TaskerId }, ct);

                if (locations.TryGetValue(card.TaskerId, out var loc))
                {
                    card.ProvinceCode = loc.ProvinceCode;
                    card.DistrictCode = loc.DistrictCode;
                    card.DistanceKm = TaskerLocationResolver.DistanceKm(
                        request.CustomerLat, request.CustomerLng, loc.Lat, loc.Lng);
                }
            }

            return ApiResponse<ServiceTaskerSuggestionDto?>.Success(
                card, "Lấy thẻ thợ theo dịch vụ thành công.");
        }
    }
}
