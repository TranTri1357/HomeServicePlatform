using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
    {
        private readonly IGenericRepository<Token> _tokenRepo;
        private readonly IUnitOfWork _unitOfWork;

        public LogoutCommandHandler(IGenericRepository<Token> tokenRepo, IUnitOfWork unitOfWork)
        {
            _tokenRepo = tokenRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var existingToken = (await _tokenRepo.FindAsync(t => t.TokenString == request.RefreshToken && t.Type == 1)).FirstOrDefault();

            if (existingToken == null)
            {
                return ApiResponse<bool>.Failure("Token không hợp lệ hoặc không tồn tại.");
            }

            if (existingToken.IsRevoked)
            {
                return ApiResponse<bool>.Failure("Phiên làm việc này đã được đăng xuất trước đó.");
            }

            //Tiến hành thu hồi token
            existingToken.IsRevoked = true;
            existingToken.RevokedAt = DateTimeOffset.UtcNow; 

            _tokenRepo.Update(existingToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return ApiResponse<bool>.Success(true, "Đăng xuất thành công");
        }
    }
}
