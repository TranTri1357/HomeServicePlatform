using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerServiceOptions
{
    public class GetTaskerServiceOptionsQueryHandler
        : IRequestHandler<GetTaskerServiceOptionsQuery, ApiResponse<List<TaskerServiceOptionDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetTaskerServiceOptionsQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<List<TaskerServiceOptionDto>>> Handle(GetTaskerServiceOptionsQuery request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var query = from ts in _context.TaskerServices
                        where ts.TaskerId == request.TaskerId
                        join s in _context.Services on ts.ServiceId equals s.ServiceId
                        where s.IsActive && !s.IsDeleted
                        join c in _context.Categories on s.CategoryId equals c.CategoryId
                        select new TaskerServiceOptionDto(
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
                            s.DurationMinutes);

            var items = await query.ToListAsync(ct);

            return ApiResponse<List<TaskerServiceOptionDto>>.Success(items, "Lấy dịch vụ của thợ thành công.");
        }
    }
}
