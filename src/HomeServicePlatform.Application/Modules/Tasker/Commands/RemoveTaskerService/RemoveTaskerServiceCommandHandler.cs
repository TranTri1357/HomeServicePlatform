using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.RemoveTaskerService
{
    public class RemoveTaskerServiceCommandHandler : IRequestHandler<RemoveTaskerServiceCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public RemoveTaskerServiceCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(RemoveTaskerServiceCommand request, CancellationToken cancellationToken)
        {
            // 1. Tìm và xóa mối quan hệ Many-to-Many ở bảng trung gian gốc
            var taskerService = await _context.TaskerServices
                .FirstOrDefaultAsync(ts => ts.TaskerId == request.TaskerId && ts.ServiceId == request.ServiceId, cancellationToken);

            if (taskerService == null)
                throw new NotFoundException("Dịch vụ này chưa từng được đăng ký cho thợ.");

            _context.TaskerServices.Remove(taskerService);

            // 2. Tìm dòng giá đang hoạt động và cập nhật ngày đóng hiệu lực (EffectiveTo) thành thời điểm hiện tại
            var activePrice = await _context.TaskerServicePrices
                .FirstOrDefaultAsync(p => p.TaskerId == request.TaskerId
                                       && p.ServiceId == request.ServiceId
                                       && p.EffectiveTo == null, cancellationToken);

            if (activePrice != null)
            {
                activePrice.EffectiveTo = DateTimeOffset.UtcNow; // Ghi nhận thời điểm hủy dịch vụ/khóa giá
            }

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.Success(true, "Hủy đăng ký dịch vụ và đóng hiệu lực bảng giá thành công.");
        }
    }
}
