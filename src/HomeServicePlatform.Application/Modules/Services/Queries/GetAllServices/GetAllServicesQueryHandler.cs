using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Services.Queries.GetAllServices
{
    public class GetAllServicesQueryHandler : IRequestHandler<GetAllServicesQuery, ApiResponse<PagedResult<ServiceDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetAllServicesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<ServiceDto>>> Handle(GetAllServicesQuery request, CancellationToken cancellationToken)
        {
            // 1. Lấy dữ liệu dạng IQueryable và bỏ qua các record đã xóa mềm
            var query = _context.Services
                .AsNoTracking() // Tăng tốc độ đọc 
                .Where(x => !x.IsDeleted);

            // 2. Tìm kiếm theo tên (Nếu có nhập searchTerm)
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search));
            }

            // 3. Đếm tổng số lượng để phục vụ phân trang
            var totalCount = await query.CountAsync(cancellationToken);

            // 4. Cắt dữ liệu (Pagination) và ánh xạ sang DTO
            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((request.PageIndex - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(x => new ServiceDto(x.ServiceId, x.Name, x.Description, x.DurationMinutes, x.IsActive))
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ServiceDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResult<ServiceDto>>.Success(result, "Lấy danh sách thành công");
        }
    }
}
