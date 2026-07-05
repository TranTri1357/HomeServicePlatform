using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Commands.CreateCategory
{
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, ApiResponse<int>>
    {
        private readonly IApplicationDbContext _context;
        public CreateCategoryCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<int>> Handle(CreateCategoryCommand request, CancellationToken ct)
        {
            var category = new Domain.Modules.Services.Entities.Category
            {
                Name = request.Name,
                Slug = request.Slug,
                IconUrl = request.IconUrl,
                IsActive = true,
                IsDeleted = false
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<int>.Success(category.CategoryId, "Thêm loại dịch vụ thành công.", 201);
        }
    }
}
