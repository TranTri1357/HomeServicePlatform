using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerProfile
{
    public class UpdateTaskerProfileCommandHandler
        : IRequestHandler<UpdateTaskerProfileCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateTaskerProfileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateTaskerProfileCommand request, CancellationToken ct)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.UserId && !u.IsDeleted, ct);

            if (user == null)
                throw new NotFoundException($"Không tìm thấy tài khoản #{request.UserId}.");

            var profile = await _context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId && !t.IsDeleted, ct);

            if (profile == null)
                throw new NotFoundException("Bạn chưa có hồ sơ thợ. Vui lòng tạo hồ sơ trước.");

            var newPhone = request.Phone.Trim();

            // Chặn trùng số điện thoại với tài khoản khác.
            bool phoneTaken = await _context.Users
                .AnyAsync(u => u.Phone == newPhone && u.UserId != request.UserId && !u.IsDeleted, ct);
            if (phoneTaken)
                throw new BadRequestException("Số điện thoại này đã được sử dụng bởi tài khoản khác.");

            user.FullName = request.FullName.Trim();
            user.Phone = newPhone;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            profile.Bio = string.IsNullOrWhiteSpace(request.Bio) ? null : request.Bio.Trim();
            profile.ExperienceYears = request.ExperienceYears;

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật hồ sơ thành công.");
        }
    }
}
