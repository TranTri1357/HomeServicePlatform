using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.SetAvailability
{
    public class SetTaskerAvailabilityCommandHandler
        : IRequestHandler<SetTaskerAvailabilityCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public SetTaskerAvailabilityCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(SetTaskerAvailabilityCommand request, CancellationToken ct)
        {
            var profile = await _context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted, ct);

            if (profile == null)
                throw new NotFoundException("Không tìm thấy hồ sơ thợ.");

            // Hồ sơ chưa được duyệt (Status 0 / chưa xác minh) không được tự bật nhận việc —
            // tránh việc thợ tự "nhảy cóc" qua bước admin phê duyệt.
            if (!profile.IsVerified || profile.Status == 0)
                throw new BadRequestException("Hồ sơ chưa được duyệt, chưa thể nhận việc. Vui lòng chờ quản trị viên phê duyệt.");

            // Bị admin khóa (Status 2) thì thợ không thể tự mở lại.
            if (profile.Status == 2)
                throw new BadRequestException("Hồ sơ đang bị khóa bởi quản trị viên.");

            // 🏠 Gate địa chỉ: chỉ khi BẬT nhận việc mới bắt buộc có địa chỉ hoạt động
            // (Phương án B: dùng chung bảng Address, khóa theo UserId == TaskerProfileId).
            // Tắt/tạm nghỉ thì không cần — thợ luôn được phép ẩn khỏi tìm thợ.
            if (request.IsAvailable)
            {
                bool hasAddress = await _context.Addresses
                    .AnyAsync(a => a.UserId == request.TaskerId, ct);
                if (!hasAddress)
                    throw new BadRequestException("Vui lòng cập nhật địa chỉ hoạt động trước khi bật nhận việc.");
            }

            // 1 = đang nhận việc (hiện trong tìm thợ gần), 3 = tạm nghỉ (ẩn, KHÁC với 0 = chờ duyệt).
            profile.Status = (short)(request.IsAvailable ? 1 : 3);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(request.IsAvailable, request.IsAvailable
                ? "Đã bật nhận việc."
                : "Đã tắt nhận việc.");
        }
    }
}
