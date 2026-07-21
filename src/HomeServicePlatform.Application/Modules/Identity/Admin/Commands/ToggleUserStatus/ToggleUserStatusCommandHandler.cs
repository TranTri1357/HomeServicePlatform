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

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Commands.ToggleUserStatus
{
    public class ToggleUserStatusCommandHandler : IRequestHandler<ToggleUserStatusCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService; 

        public ToggleUserStatusCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<ApiResponse<bool>> Handle(ToggleUserStatusCommand request, CancellationToken ct)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId, ct);

            if (user == null || user.IsDeleted)
                throw new NotFoundException("Tài khoản không tồn tại hoặc đã bị xóa vĩnh viễn.");

            if (request.UserId == _currentUserService.UserId)
                throw new BadRequestException("Bạn không thể tự khóa tài khoản của chính mình.");

            string message;
            if (user.Status == 1)
            {
                user.Status = 0; 
                message = $"Đã khóa tài khoản của {user.FullName} thành công.";
            }
            else
            {
                user.Status = 1;
                message = $"Đã mở khóa tài khoản của {user.FullName} thành công.";
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, message);
        }
    }
}
