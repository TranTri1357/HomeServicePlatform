using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Admin.Dtos;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetAllUsers
{
    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ApiResponse<PagedResult<UserDto>>>
    {
        private readonly IApplicationDbContext _context;
        public GetAllUsersQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PagedResult<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken ct)
        {
            var query = _context.Users
                .AsNoTracking()
                .Where(u => !u.IsDeleted);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var search = request.SearchTerm.ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.Phone.Contains(search) 
                );
            }

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((PageSizeGuard.ClampIndex(request.PageIndex) - 1) * PageSizeGuard.Clamp(request.PageSize))
                .Take(PageSizeGuard.Clamp(request.PageSize))
                .Select(u => new UserDto(
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Phone,

                    _context.Bookings.Count(b => b.CustomerId == u.UserId),

                    _context.Bookings
                        .Where(b => b.CustomerId == u.UserId && b.Status == BookingStatus.Completed)
                        .Sum(b => (decimal?)b.FinalAmount) ?? 0,

                    u.CreatedAt,
                    u.Status
                ))
                .ToListAsync(ct);

            var result = new PagedResult<UserDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = PageSizeGuard.ClampIndex(request.PageIndex),
                PageSize = PageSizeGuard.Clamp(request.PageSize)
            };

            return ApiResponse<PagedResult<UserDto>>.Success(result, "Lấy danh sách khách hàng thành công.");
        }
    }
}
