using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Services.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServiceDetail
{
    public class GetServiceDetailQueryHandler : IRequestHandler<GetServiceDetailQuery, ApiResponse<ServiceDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetServiceDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<ServiceDetailDto>> Handle(GetServiceDetailQuery request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var serviceDetail = await _context.Services
                .AsNoTracking()
                .Where(s => s.ServiceId == request.ServiceId && s.IsActive && !s.IsDeleted)
                .Select(s => new ServiceDetailDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,
                    Description = s.Description,
                    DurationMinutes = s.DurationMinutes,
                    TotalBookings = s.BookingItems.Count(),

                    StartingPrice = s.TaskerServicePrices
                        .Where(p => p.EffectiveFrom <= now && (p.EffectiveTo == null || p.EffectiveTo > now))
                        .Min(p => (decimal?)p.Price) ?? 0,

                    ImageUrl = s.ImageUrl,

                    SuggestedTaskers = s.TaskerServices
                        .Where(ts => !ts.TaskerProfile.IsDeleted && ts.TaskerProfile.Status == 1)
                        .OrderByDescending(ts => ts.TaskerProfile.RatingAvg)
                        .Take(5)
                        .Select(ts => new ServiceTaskerSuggestionDto
                        {
                            TaskerId = ts.TaskerId,
                            FullName = ts.TaskerProfile.User.FullName,
                            ExperienceYears = ts.TaskerProfile.ExperienceYears,
                            RatingAvg = ts.TaskerProfile.RatingAvg,
                            AvatarUrl = null,

                            CurrentPrice = ts.TaskerProfile.TaskerServicePrices
                                .Where(p => p.ServiceId == s.ServiceId
                                            && p.EffectiveFrom <= now
                                            && (p.EffectiveTo == null || p.EffectiveTo > now))
                                .OrderByDescending(p => p.EffectiveFrom)
                                .Select(p => p.Price)
                                .FirstOrDefault()
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (serviceDetail == null)
            {
                throw new NotFoundException($"Không tìm thấy dịch vụ hoặc dịch vụ này đã ngừng hoạt động.");
            }

            return ApiResponse<ServiceDetailDto>.Success(serviceDetail, "Lấy chi tiết dịch vụ thành công");
        }
    }
}
