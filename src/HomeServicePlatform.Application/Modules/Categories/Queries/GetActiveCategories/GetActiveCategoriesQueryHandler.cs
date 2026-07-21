using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Categories.Dtos;
using HomeServicePlatform.Domain.Modules.Services.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Queries.GetActiveCategories
{
    public class GetActiveCategoriesQueryHandler : IRequestHandler<GetActiveCategoriesQuery, ApiResponse<List<CategoryDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetActiveCategoriesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<CategoryDto>>> Handle(GetActiveCategoriesQuery request, CancellationToken cancellationToken)
        {
            IQueryable<Category> query = _context.Categories
                .AsNoTracking()
                .Where(c => c.IsActive == true && !c.IsDeleted)
                .OrderBy(c => c.CategoryId);

            if (request.Limit.HasValue)
            {
                query = query.Take(request.Limit.Value);
            }

            var categories = await query
                .Select(c => new CategoryDto
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Slug = c.Slug,
                    IconUrl = c.IconUrl
                })
                .ToListAsync(cancellationToken);

            return ApiResponse<List<CategoryDto>>.Success(categories, "Lấy danh mục thành công");
        }
    }
}
