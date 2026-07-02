using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Services.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Commands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Service> _repo;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteServiceCommandHandler(IGenericRepository<Service> repo, IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _repo.GetByIdAsync(request.ServiceId);

            if (service == null || service.IsDeleted)
                throw new NotFoundException("Dịch vụ không tồn tại hoặc đã bị xóa.");

            service.IsDeleted = true;


            _repo.Update(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Xóa dịch vụ thành công.");
        }
    }
}
