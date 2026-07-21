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

namespace HomeServicePlatform.Application.Modules.Identity.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResultDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IGenericRepository<Token> _tokenRepo;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IUnitOfWork _unitOfWork;

        public RefreshTokenCommandHandler(IApplicationDbContext context, IGenericRepository<Token> tokenRepo, IJwtTokenGenerator jwtTokenGenerator, IUnitOfWork unitOfWork)
        {
            _context = context; _tokenRepo = tokenRepo; _jwtTokenGenerator = jwtTokenGenerator; _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<AuthResultDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var existingToken = await _context.Tokens.FirstOrDefaultAsync(t => t.TokenString == request.RefreshToken && t.Type == 1, cancellationToken);

            if (existingToken == null || existingToken.ExpiredAt < DateTimeOffset.UtcNow)
                throw new UnauthorizedException("Refresh token không hợp lệ hoặc đã hết hạn.");

            if (existingToken.IsRevoked)
            {
                var allUserTokens = await _context.Tokens.Where(t => t.UserId == existingToken.UserId && !t.IsRevoked).ToListAsync(cancellationToken);
                foreach (var token in allUserTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTimeOffset.UtcNow;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                throw new UnauthorizedException("Phát hiện truy cập bất thường. Vui lòng đăng nhập lại.");
            }

            var user = await _context.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == existingToken.UserId && !u.IsDeleted, cancellationToken);

            if (user == null) throw new NotFoundException("Tài khoản không tồn tại.");
            if (user.Status == 0) throw new ForbiddenException("Tài khoản đã bị khóa.");

            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTimeOffset.UtcNow;

            var roles = user.UserRoles.Select(ur => ur.Role.RoleName).ToList();
            if (roles.Count == 0) roles = new List<string> { "Customer" };

            var (newAccessToken, accessTokenExpiresAt) = _jwtTokenGenerator.GenerateToken(user, roles);
            var newRefreshTokenStr = _jwtTokenGenerator.GenerateRefreshToken();
            var refreshTokenExpiresAt = DateTimeOffset.UtcNow.AddDays(7);

            await _tokenRepo.AddAsync(new Token
            {
                UserId = user.UserId,
                TokenString = newRefreshTokenStr,
                ExpiredAt = refreshTokenExpiresAt,
                Type = 1,
                IsRevoked = false
            });

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<AuthResultDto>.Success(new AuthResultDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Roles = roles,
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenStr,
                AccessTokenExpiresAt = accessTokenExpiresAt,
                RefreshTokenExpiresAt = refreshTokenExpiresAt.UtcDateTime
            }, "Làm mới token thành công");
        }
    }
}
