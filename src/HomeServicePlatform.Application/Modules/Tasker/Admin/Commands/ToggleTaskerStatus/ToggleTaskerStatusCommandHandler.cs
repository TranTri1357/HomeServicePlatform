using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.ToggleTaskerStatus
{
    public class ToggleTaskerStatusCommandHandler : IRequestHandler<ToggleTaskerStatusCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public ToggleTaskerStatusCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(ToggleTaskerStatusCommand request, CancellationToken ct)
        {
            var tasker = await _context.TaskerProfiles
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId, ct);

            if (tasker == null || tasker.IsDeleted)
                throw new NotFoundException("Không tìm thấy hồ sơ thợ hoặc hồ sơ đã bị xóa.");

            if (tasker.Status == 0)
                throw new BadRequestException("Hồ sơ đang chờ duyệt, vui lòng sử dụng chức năng Phê duyệt hoặc Từ chối.");

            string message;

            if (tasker.Status == 1)
            {
                tasker.Status = 2;
                message = $"Đã khóa hồ sơ làm việc của thợ {tasker.User.FullName}.";
            }
            else
            {
                tasker.Status = 1;
                message = $"Đã mở khóa hồ sơ làm việc cho thợ {tasker.User.FullName}.";
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, message);
        }
    }
}
