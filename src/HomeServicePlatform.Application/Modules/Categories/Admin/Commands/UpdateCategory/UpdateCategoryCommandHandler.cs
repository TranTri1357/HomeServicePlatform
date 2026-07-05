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

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Commands.UpdateCategory
{
    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public UpdateCategoryCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(UpdateCategoryCommand request, CancellationToken ct)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, ct);

            if (category == null || category.IsDeleted)
                throw new NotFoundException("Không tìm thấy loại dịch vụ.");

            category.Name = request.Name;
            category.Slug = request.Slug;
            category.IconUrl = request.IconUrl;
            category.IsActive = request.IsActive;

            // SaveChangesAsync sẽ tự động lưu UpdatedAt nhờ cơ chế Audit Trail của bạn
            await _context.SaveChangesAsync(ct);
            return ApiResponse<bool>.Success(true, "Cập nhật thành công.");
        }
    }
}
