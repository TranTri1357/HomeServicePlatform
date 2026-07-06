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

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.ApproveTasker
{
    public class ApproveTaskerCommandHandler : IRequestHandler<ApproveTaskerCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public ApproveTaskerCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(ApproveTaskerCommand request, CancellationToken ct)
        {
            var tasker = await _context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId, ct);

            if (tasker == null || tasker.IsDeleted)
                throw new NotFoundException("Không tìm thấy hồ sơ thợ hoặc hồ sơ đã bị xóa.");

            tasker.VerifyTasker();

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Phê duyệt hồ sơ thợ thành công.");
        }
    }
}
