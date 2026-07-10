using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.SetAvailability
{
    public class SetTaskerAvailabilityCommandHandler
        : IRequestHandler<SetTaskerAvailabilityCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public SetTaskerAvailabilityCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(SetTaskerAvailabilityCommand request, CancellationToken ct)
        {
            var profile = await _context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted, ct);

            if (profile == null)
                throw new NotFoundException("Không tìm thấy hồ sơ thợ.");

            // 1 = đang nhận việc (hiện trong tìm thợ gần), 0 = tạm nghỉ (ẩn).
            profile.Status = (short)(request.IsAvailable ? 1 : 0);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(request.IsAvailable, request.IsAvailable
                ? "Đã bật nhận việc."
                : "Đã tắt nhận việc.");
        }
    }
}
