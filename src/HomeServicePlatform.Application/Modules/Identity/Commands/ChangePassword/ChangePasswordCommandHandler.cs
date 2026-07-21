using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public ChangePasswordCommandHandler(
            IApplicationDbContext context, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(ChangePasswordCommand request, CancellationToken ct)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => !u.IsDeleted && u.UserId == request.UserId, ct);

            if (user == null)
                throw new NotFoundException("Không tìm thấy tài khoản.");

            if (!_passwordHasher.Verify(request.OldPassword, user.PasswordHash))
                throw new BadRequestException("Mật khẩu hiện tại không đúng.");

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            user.UpdatedAt = System.DateTimeOffset.UtcNow;

            var now = System.DateTimeOffset.UtcNow;
            var activeTokens = await _context.Tokens
                .Where(t => t.UserId == user.UserId && t.Type == 1 && !t.IsRevoked)
                .ToListAsync(ct);
            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = now;
            }

            await _unitOfWork.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Đổi mật khẩu thành công. Vui lòng đăng nhập lại trên các thiết bị khác.");
        }
    }
}
