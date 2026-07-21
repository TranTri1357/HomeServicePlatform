using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobStats
{
    public class GetTaskerJobStatsQueryHandler
        : IRequestHandler<GetTaskerJobStatsQuery, ApiResponse<TaskerJobStatsDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerJobStatsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerJobStatsDto>> Handle(GetTaskerJobStatsQuery request, CancellationToken ct)
        {
            var taskerId = request.TaskerId;

            var baseQuery = _context.Bookings
                .AsNoTracking()
                .Where(b => b.BookingItems.Any(i => i.TaskerId == taskerId)
                            && _context.Payments.Any(p => p.BookingId == b.BookingId
                                && (p.Status == (short)PaymentStatus.Paid
                                    || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))))
                .Select(b => (short)b.Status);

            var incoming = await baseQuery.CountAsync(s => s == 0, ct);
            var active = await baseQuery.CountAsync(s => s >= 1 && s <= 3, ct);
            var history = await baseQuery.CountAsync(s => s >= 4, ct);

            var dto = new TaskerJobStatsDto(incoming, active, history);
            return ApiResponse<TaskerJobStatsDto>.Success(dto, "Lấy thống kê công việc của thợ thành công.");
        }
    }
}
