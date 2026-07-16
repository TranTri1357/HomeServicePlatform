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

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.RejectTasker
{
    public class RejectTaskerCommandHandler : IRequestHandler<RejectTaskerCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public RejectTaskerCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(RejectTaskerCommand request, CancellationToken ct)
        {
            var tasker = await _context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId, ct);

            if (tasker == null || tasker.IsDeleted)
                throw new NotFoundException("Không tìm thấy hồ sơ thợ hoặc hồ sơ đã bị xóa.");

            if (tasker.Status == 1)
                throw new BadRequestException("Không thể từ chối hồ sơ thợ đã được duyệt và đang hoạt động.");

            tasker.RejectProfile(request.Reason);

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Đã từ chối hồ sơ thợ thành công.");
        }
    }
}
