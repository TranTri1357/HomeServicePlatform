using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.CreateTimeOff
{
    public class CreateTimeOffCommandHandler : IRequestHandler<CreateTimeOffCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public CreateTimeOffCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CreateTimeOffCommand request, CancellationToken ct)
        {
            var taskerProfile = await _context.TaskerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId, ct);

            if (taskerProfile == null)
                throw new NotFoundException("Bạn chưa hoàn thiện hồ sơ thợ.");

            if (taskerProfile.Status != 1 && taskerProfile.Status != 3)
                throw new ForbiddenException("Hồ sơ của bạn chưa được duyệt, bạn chưa thể đăng ký lịch nghỉ. Vui lòng chờ quản trị viên phê duyệt hồ sơ.");

            var timeOff = new TaskerTimeOff { TaskerId = taskerProfile.TaskerProfileId };

            timeOff.RequestTimeOff(
                request.StartAt.ToUniversalTime(),
                request.EndAt.ToUniversalTime(),
                request.Reason
            );

            _context.TaskerTimeOffs.Add(timeOff);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Đã đăng ký khung giờ bận thành công.");
        }
    }
}
