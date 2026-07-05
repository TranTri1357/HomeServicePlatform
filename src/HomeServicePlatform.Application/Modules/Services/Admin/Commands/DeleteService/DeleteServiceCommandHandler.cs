using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Services.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Commands.DeleteService
{
    public class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteServiceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == request.ServiceId, cancellationToken);

            if (service == null || service.IsDeleted)
                throw new NotFoundException("Dịch vụ không tồn tại hoặc đã bị xóa.");

            service.IsDeleted = true;

            await _context.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Xóa dịch vụ thành công.");
        }
    }
}
