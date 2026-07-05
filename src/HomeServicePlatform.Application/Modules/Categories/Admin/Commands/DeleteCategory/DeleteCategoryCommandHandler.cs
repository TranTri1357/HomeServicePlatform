using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Commands.DeleteCategory
{
    public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public DeleteCategoryCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(DeleteCategoryCommand request, CancellationToken ct)
        {
            var category = await _context.Categories
                .Include(c => c.Services)
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, ct);

            if (category == null || category.IsDeleted)
                throw new NotFoundException("Loại dịch vụ không tồn tại hoặc đã bị xóa.");

            if (category.Services.Any(s => !s.IsDeleted))
            {
                throw new BadRequestException("Không thể xóa! Loại dịch vụ này vẫn còn chứa các Dịch vụ con đang hoạt động.");
            }

            category.IsDeleted = true;

            await _context.SaveChangesAsync(ct);
            return ApiResponse<bool>.Success(true, "Xóa loại dịch vụ thành công.");
        }
    }
}
