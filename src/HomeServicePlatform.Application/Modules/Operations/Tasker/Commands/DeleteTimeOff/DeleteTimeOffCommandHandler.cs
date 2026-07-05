using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.DeleteTimeOff
{
    public class DeleteTimeOffCommandHandler : IRequestHandler<DeleteTimeOffCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteTimeOffCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteTimeOffCommand request, CancellationToken ct)
        {
            var taskerProfile = await _context.TaskerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId, ct);
            if (taskerProfile == null) throw new Exception("Bạn chưa hoàn thiện hồ sơ thợ.");

            var timeOff = await _context.TaskerTimeOffs
                .FirstOrDefaultAsync(t => t.TimeOffId == request.TimeOffId && t.TaskerId == taskerProfile.TaskerProfileId, ct);

            if (timeOff == null) throw new Exception("Không tìm thấy lịch bận hoặc bạn không có quyền xóa.");
            if (timeOff.StartAt < DateTimeOffset.UtcNow) throw new Exception("Không thể xóa lịch bận đã diễn ra trong quá khứ.");

            _context.TaskerTimeOffs.Remove(timeOff);
            await _context.SaveChangesAsync(ct);
            return ApiResponse<bool>.Success(true, "Đã mở lại khung giờ làm việc.");
        }
    }
}
