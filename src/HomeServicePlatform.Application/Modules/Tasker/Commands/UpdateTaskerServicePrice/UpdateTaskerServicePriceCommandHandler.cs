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
            // Tìm bản ghi giá đang kích hoạt của thợ cho dịch vụ cụ thể này
            var currentPrice = await _context.TaskerServicePrices
                .FirstOrDefaultAsync(p => p.TaskerId == request.TaskerId
                                       && p.ServiceId == request.ServiceId
                                       && p.EffectiveTo == null, cancellationToken);

            if (currentPrice == null)
                throw new NotFoundException("Không tìm thấy cấu hình bảng giá đang hoạt động của thợ này.");

            // Cập nhật trực tiếp trên dòng cũ theo yêu cầu
            currentPrice.Price = request.NewPrice;
            currentPrice.EffectiveFrom = DateTimeOffset.UtcNow; // Cập nhật lại thời gian bắt đầu áp dụng giá mới

            await _context.SaveChangesAsync(cancellationToken);
            return ApiResponse<bool>.Success(true, "Cập nhật giá và thời gian hiệu lực dịch vụ thành công.");
        }
    }
}
