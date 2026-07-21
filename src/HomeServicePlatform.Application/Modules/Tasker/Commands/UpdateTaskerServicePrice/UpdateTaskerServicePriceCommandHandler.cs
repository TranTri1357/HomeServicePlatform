using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using HomeServicePlatform.Application.Modules.Tasker.Commands.RemoveTaskerService;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerServicePrice
{
    public class UpdateTaskerServicePriceCommandHandler : IRequestHandler<UpdateTaskerServicePriceCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTaskerServicePriceCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(UpdateTaskerServicePriceCommand request, CancellationToken cancellationToken)
        {
            var profile = await _context.TaskerProfiles.AsNoTracking()
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted, cancellationToken);

            if (profile == null)
                throw new NotFoundException("Bạn chưa hoàn thiện hồ sơ thợ.");

            if (profile.Status != 1 && profile.Status != 3)
                throw new ForbiddenException("Hồ sơ của bạn chưa được duyệt, bạn chưa thể cập nhật giá dịch vụ. Vui lòng chờ quản trị viên phê duyệt hồ sơ.");

            var currentPrice = await _context.TaskerServicePrices
                .Where(p => p.TaskerId == request.TaskerId && p.ServiceId == request.ServiceId)
                .ActiveAt(DateTimeOffset.UtcNow)
                .FirstOrDefaultAsync(cancellationToken);

            if (currentPrice == null)
                throw new NotFoundException("Không tìm thấy cấu hình bảng giá đang hoạt động của thợ này.");

            currentPrice.Price = request.NewPrice;
            currentPrice.EffectiveFrom = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.Success(true, "Cập nhật giá và thời gian hiệu lực dịch vụ thành công.");
        }
    }
}
