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
using ServiceEntity = HomeServicePlatform.Domain.Modules.Services.Entities.Service;


namespace HomeServicePlatform.Application.Modules.Services.Admin.Commands.UpdateService
{
    public class UpdateServiceCommandHandler : IRequestHandler<UpdateServiceCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateServiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == request.ServiceId, cancellationToken);

            if (service == null || service.IsDeleted)
                throw new NotFoundException($"Không tìm thấy dịch vụ với ID {request.ServiceId}");

            service.CategoryId = request.CategoryId;
            service.Name = request.Name;
            service.Description = request.Description;
            service.ImageUrl = request.ImageUrl;
            service.DurationMinutes = request.DurationMinutes;
            service.IsActive = request.IsActive;

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Cập nhật thông tin dịch vụ thành công.");
        }
    }
}
