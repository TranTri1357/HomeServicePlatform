using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.AddTaskerService
{
    public class AddTaskerServiceCommandHandler : IRequestHandler<AddTaskerServiceCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public AddTaskerServiceCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(AddTaskerServiceCommand request, CancellationToken cancellationToken)
        {
            var exists = await _context.TaskerServices
                .AnyAsync(ts => ts.TaskerId == request.TaskerId && ts.ServiceId == request.ServiceId, cancellationToken);

            if (exists) throw new BadRequestException("Thợ đã đăng ký gói dịch vụ này từ trước.");

            var taskerService = new TaskerService
            {
                TaskerId = request.TaskerId,
                ServiceId = request.ServiceId
            };

            var taskerServicePrice = new TaskerServicePrice
            {
                TaskerId = request.TaskerId,
                ServiceId = request.ServiceId,
                Price = request.Price,
                EffectiveFrom = DateTimeOffset.UtcNow,
                EffectiveTo = null
            };

            _context.TaskerServices.Add(taskerService);
            _context.TaskerServicePrices.Add(taskerServicePrice);

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Đăng ký dịch vụ cho thợ thành công.");
        }
    }
}
