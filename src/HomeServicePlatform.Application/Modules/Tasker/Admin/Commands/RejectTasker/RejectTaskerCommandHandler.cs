using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
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

            if (tasker.Status != 0)
                throw new BadRequestException("Chỉ có thể từ chối hồ sơ đang ở trạng thái chờ duyệt.");

            tasker.RejectProfile(request.Reason);

            var body = string.IsNullOrWhiteSpace(request.Reason)
                ? "Hồ sơ của bạn chưa được duyệt. Vui lòng bổ sung thông tin và nộp lại."
                : $"Hồ sơ của bạn chưa được duyệt. Lý do: {request.Reason}. Vui lòng bổ sung và nộp lại.";
            _context.Notifications.Add(NotificationBuilder.Build(
                tasker.TaskerProfileId,
                NotificationType.ProfileRejected,
                "Hồ sơ chưa được duyệt",
                body));

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Đã từ chối hồ sơ thợ thành công.");
        }
    }
}
