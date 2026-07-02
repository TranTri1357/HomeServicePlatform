using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Identity.Dtos;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IGenericRepository<Token> _tokenRepo;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public LoginCommandHandler(IApplicationDbContext context, IGenericRepository<Token> tokenRepo, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork)
        {
            _context = context; _tokenRepo = tokenRepo; _passwordHasher = passwordHasher; _jwtTokenGenerator = jwtTokenGenerator; _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<AuthResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var identifier = request.Identifier.Trim().ToLower();

            var user = await _context.Users
                .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => !u.IsDeleted && (u.Email == identifier || u.Phone == identifier), cancellationToken);

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedException("Tài khoản hoặc mật khẩu không đúng.");

            if (user.Status == 0)
                throw new ForbiddenException("Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.");

            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            if (roles.Count == 0) roles = new List<string> { "Customer" };

            var accessToken = _jwtTokenGenerator.GenerateToken(user, roles);
            var refreshTokenStr = _jwtTokenGenerator.GenerateRefreshToken();

            var accessTokenExpiresAt = DateTimeOffset.UtcNow.AddMinutes(60);
            var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

            await _tokenRepo.AddAsync(new Token
            {
                UserId = user.UserId,
                TokenString = refreshTokenStr,
                ExpiredAt = refreshTokenExpiresAt,
                Type = 1,
                IsRevoked = false
            });

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<AuthResultDto>.Success(new AuthResultDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Roles = roles,
                AccessToken = accessToken,
                RefreshToken = refreshTokenStr,
                AccessTokenExpiresAt = accessTokenExpiresAt.UtcDateTime,
                RefreshTokenExpiresAt = refreshTokenExpiresAt.UtcDateTime
            }, "Đăng nhập thành công");
        }
    }
}
