using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Categories.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Queries.GetCategoryDetail
{
    public class GetCategoryDetailQueryHandler : IRequestHandler<GetCategoryDetailQuery, ApiResponse<CategoryDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCategoryDetailQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<CategoryDetailDto>> Handle(GetCategoryDetailQuery request, CancellationToken ct)
        {
            var category = await _context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == request.CategoryId && !c.IsDeleted)
                .Select(c => new CategoryDetailDto(c.CategoryId, c.Name, c.Slug, c.IconUrl, c.IsActive ?? false))
                .FirstOrDefaultAsync(ct);

            if (category == null) throw new NotFoundException("Không tìm thấy loại dịch vụ.");
            return ApiResponse<CategoryDetailDto>.Success(category, "Lấy thông tin thành công.");
        }
    }
}
