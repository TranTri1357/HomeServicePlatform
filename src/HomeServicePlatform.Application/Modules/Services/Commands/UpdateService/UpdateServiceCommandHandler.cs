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


namespace HomeServicePlatform.Application.Modules.Services.Commands.UpdateService
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
            // 1. Tìm đối tượng gốc từ Database lên
            var service = await _repo.GetByIdAsync(request.ServiceId);

            // 2. Kiểm tra nếu không có hoặc đã bị xóa mềm trước đó
            if (service == null || service.IsDeleted)
                throw new NotFoundException($"Không tìm thấy dịch vụ với ID {request.ServiceId}");

            // 3. Tiến hành cập nhật thông tin mới
            service.CategoryId = request.CategoryId;
            service.Name = request.Name;
            service.Description = request.Description;
            service.DurationMinutes = request.DurationMinutes;
            service.IsActive = request.IsActive;
            service.UpdatedAt = DateTimeOffset.UtcNow; // Cập nhật dấu mốc thời gian sửa sửa

            // 4. Đánh dấu thay đổi và chốt hạ lưu xuống Postgres
            _repo.Update(service);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Cập nhật thông tin dịch vụ thành công.");
        }
    }
}
