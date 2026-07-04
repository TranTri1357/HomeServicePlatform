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

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.MarkNotificationRead
{
    public class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public MarkNotificationReadCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(MarkNotificationReadCommand request, CancellationToken ct)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.NotificationId == request.NotificationId && n.UserId == request.UserId, ct);

            if (notification == null)
                throw new NotFoundException("Không tìm thấy thông báo.");

            if (notification.Status != 1)
            {
                notification.Status = 1;
                await _context.SaveChangesAsync(ct);
            }

            return ApiResponse<bool>.Success(true, "Đã đánh dấu đọc.");
        }
    }
}
