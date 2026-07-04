using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTopTaskers
{
    public class GetTopTaskersQueryHandler : IRequestHandler<GetTopTaskersQuery, ApiResponse<List<TopTaskerDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetTopTaskersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<TopTaskerDto>>> Handle(GetTopTaskersQuery request, CancellationToken ct)
        {
            var topTaskers = await _context.TaskerProfiles
                .AsNoTracking()
                .Where(t => t.Status == 1 && t.IsVerified && !t.IsDeleted)
                .OrderByDescending(t => t.RatingAvg)
                .ThenByDescending(t => t.TotalReviews)
                .Take(request.Limit)
                .Select(t => new TopTaskerDto
                {
                    TaskerId = t.TaskerProfileId,
                    FullName = t.User.FullName,
                    AvatarUrl = null, // Chờ cập nhật logic lưu file
                    RatingAvg = t.RatingAvg,
                    TotalReviews = t.TotalReviews,
                    IsVerified = t.IsVerified,

                    // Lấy ngẫu nhiên 1 dịch vụ (Service) mà thợ đang làm để gán làm "Chuyên môn chính"
                    MainSkill = t.TaskerServices
                                 .Where(ts => ts.Service.IsActive && !ts.Service.IsDeleted)
                                 .Select(ts => ts.Service.Name)
                                 .FirstOrDefault()
                })
                .ToListAsync(ct);

            return ApiResponse<List<TopTaskerDto>>.Success(topTaskers, "Lấy danh sách thợ nổi bật thành công.");
        }
    }
}
