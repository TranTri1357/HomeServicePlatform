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
        private readonly IGenericRepository<ServiceEntity> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public CreateServiceCommandHandler(IGenericRepository<ServiceEntity> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
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

            await _repo.AddAsync(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<long>.Success(service.ServiceId, "Tạo dịch vụ thành công.", 201);
        }
    }
}
