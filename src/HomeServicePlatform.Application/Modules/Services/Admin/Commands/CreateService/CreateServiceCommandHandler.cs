using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceEntity = HomeServicePlatform.Domain.Modules.Services.Entities.Service;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Commands.CreateService
{
    public class CreateServiceCommandHandler : IRequestHandler<CreateServiceCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;

        public CreateServiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<long>> Handle(CreateServiceCommand request, CancellationToken cancellationToken)
        {
            var service = new ServiceEntity
            {
                CategoryId = request.CategoryId,
                Name = request.Name,
                Description = request.Description,
                DurationMinutes = request.DurationMinutes,
                IsActive = true,
                IsDeleted = false
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<long>.Success(service.ServiceId, "Tạo dịch vụ thành công.", 201);
        }
    }
}
