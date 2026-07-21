using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerServices
{
    public class GetTaskerServicesQueryHandler : IRequestHandler<GetTaskerServicesQuery, ApiResponse<List<TaskerServiceDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerServicesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<TaskerServiceDto>>> Handle(GetTaskerServicesQuery request, CancellationToken cancellationToken)
        {
            var now = DateTimeOffset.UtcNow;

            var query = from ts in _context.TaskerServices
                        where ts.TaskerId == request.TaskerProfileId

                        join s in _context.Services on ts.ServiceId equals s.ServiceId
                        join c in _context.Categories on s.CategoryId equals c.CategoryId

                        select new TaskerServiceDto(
                            ts.TaskerId,
                            s.ServiceId,
                            s.Name,
                            c.Name,
                            _context.TaskerServicePrices
                                .Where(p => p.TaskerId == ts.TaskerId && p.ServiceId == ts.ServiceId
                                            && p.EffectiveFrom <= now
                                            && (p.EffectiveTo == null || p.EffectiveTo > now))
                                .OrderByDescending(p => p.EffectiveFrom)
                                .Select(p => p.Price)
                                .FirstOrDefault(),
                            s.DurationMinutes,
                            s.IsActive,
                            s.ImageUrl
                        );

            var result = await query.ToListAsync(cancellationToken);

            return ApiResponse<List<TaskerServiceDto>>.Success(result, "Lấy danh sách dịch vụ của thợ thành công.");
        }
    }
}
