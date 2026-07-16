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

namespace HomeServicePlatform.Application.Modules.Services.Public.Queries.GetPopularServices
{
    public class GetPopularServicesQueryHandler : IRequestHandler<GetPopularServicesQuery, ApiResponse<List<PopularServiceDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetPopularServicesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<PopularServiceDto>>> Handle(GetPopularServicesQuery request, CancellationToken cancellationToken)
        {
            var popularServices = await _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive && !s.IsDeleted)
                .Select(s => new PopularServiceDto
                {
                    ServiceId = s.ServiceId,
                    Name = s.Name,

                    TotalBookings = s.BookingItems.Count(),

                    // Tìm giá khởi điểm (MIN Price)
                    // Chỉ lấy những mức giá đang có hiệu lực (EffectiveTo là null hoặc lớn hơn hiện tại)
                    StartingPrice = s.TaskerServicePrices
                        .Where(p => p.EffectiveTo == null || p.EffectiveTo > DateTimeOffset.UtcNow)
                        .Min(p => (decimal?)p.Price) ?? 0,

                    ImageUrl = s.ImageUrl
                })
                .OrderByDescending(s => s.TotalBookings)
                .Take(request.Limit)
                .ToListAsync(cancellationToken);

            return ApiResponse<List<PopularServiceDto>>.Success(popularServices, "Lấy dịch vụ phổ biến thành công");
        }
    }
}
