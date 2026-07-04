using HomeServicePlatform.Application.Common.Exceptions;
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

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerQuickInfo
{
    public class GetTaskerQuickInfoQueryHandler : IRequestHandler<GetTaskerQuickInfoQuery, ApiResponse<TaskerQuickInfoDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerQuickInfoQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerQuickInfoDto>> Handle(GetTaskerQuickInfoQuery request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var quickInfo = await _context.TaskerProfiles
                .AsNoTracking()
                .Where(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted)
                .Select(t => new TaskerQuickInfoDto
                {
                    TaskerId = t.TaskerProfileId,
                    FullName = t.User.FullName,
                    AvatarUrl = null,
                    RatingAvg = t.RatingAvg,
                    IsVerified = t.IsVerified,

                    MainSkill = t.TaskerServices
                                 .Where(ts => ts.ServiceId == request.ServiceId)
                                 .Select(ts => ts.Service.Name)
                                 .FirstOrDefault(),

                    CurrentPrice = t.TaskerServicePrices
                                    .Where(p => p.ServiceId == request.ServiceId &&
                                               (p.EffectiveTo == null || p.EffectiveTo > now))
                                    .Select(p => p.Price)
                                    .FirstOrDefault()
                })
                .FirstOrDefaultAsync(ct);

            if (quickInfo == null)
            {
                throw new NotFoundException("Không tìm thấy thông tin thợ hoặc thợ không cung cấp dịch vụ này.");
            }

            return ApiResponse<TaskerQuickInfoDto>.Success(quickInfo, "Lấy thông tin nhanh thành công.");
        }
    }
}
