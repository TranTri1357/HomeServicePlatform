using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Admin.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetUserDetail
{
    public class GetUserDetailQueryHandler : IRequestHandler<GetUserDetailQuery, ApiResponse<UserDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetUserDetailQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<UserDetailDto>> Handle(GetUserDetailQuery request, CancellationToken ct)
        {
            var userCore = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserId == request.UserId && !u.IsDeleted)
                .Select(u => new {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.Phone,
                    u.Status,
                    u.CreatedAt,
                    u.LastLoginAt,
                    Balance = u.Wallet != null ? u.Wallet.Balance : 0
                })
                .FirstOrDefaultAsync(ct);

            if (userCore == null)
                throw new NotFoundException($"Không tìm thấy tài khoản với ID {request.UserId}.");

            var roles = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId == request.UserId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync(ct);

            var addresses = await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == request.UserId)
                .Select(a => new UserAddressDto(
                    a.AddressId,
                    a.AddressLine,
                    a.IsDefault ?? false
                ))
                .ToListAsync(ct);

            var userDetail = new UserDetailDto(
                userCore.UserId,
                userCore.FullName,
                userCore.Email,
                userCore.Phone,
                userCore.Status,
                userCore.CreatedAt,
                userCore.LastLoginAt,
                userCore.Balance,
                roles,
                addresses
            );

            return ApiResponse<UserDetailDto>.Success(userDetail, "Lấy thông tin chi tiết thành công.");
        }
    }
}
