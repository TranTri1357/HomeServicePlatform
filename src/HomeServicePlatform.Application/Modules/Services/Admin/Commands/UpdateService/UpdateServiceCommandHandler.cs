using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
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
        private readonly IGenericRepository<ServiceEntity> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateServiceCommandHandler(IGenericRepository<ServiceEntity> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _repo.GetByIdAsync(request.ServiceId);

            if (service == null || service.IsDeleted)
                throw new NotFoundException($"Không tìm thấy dịch vụ với ID {request.ServiceId}");

            service.CategoryId = request.CategoryId;
            service.Name = request.Name;
            service.Description = request.Description;
            service.DurationMinutes = request.DurationMinutes;
            service.IsActive = request.IsActive;


            _repo.Update(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Cập nhật thông tin dịch vụ thành công.");
        }
    }
}
