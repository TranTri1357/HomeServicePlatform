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
using HomeServicePlatform.Application.Modules.Services.Admin.Dtos;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetAllServices
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

            var query = _context.Services
                .AsNoTracking()
                .Where(x => !x.IsDeleted);


            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(x => x.Name.ToLower().Contains(search));
            }


            var totalCount = await query.CountAsync(cancellationToken);


            var items = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(x => new ServiceDto(
                    x.ServiceId,
                    x.Name,
                    x.Category.Name,
                    x.TaskerServices.Count(),
                    x.BookingItems.Count(),
                    x.IsActive,
                    x.ImageUrl
                ))
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ServiceDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<ServiceDto>>.Success(result, "Lấy danh sách dịch vụ thành công");
        }
    }
}
