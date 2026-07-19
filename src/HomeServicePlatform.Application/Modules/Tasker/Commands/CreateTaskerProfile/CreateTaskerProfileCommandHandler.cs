using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using MediatR;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile
{
    public class CreateTaskerProfileCommandHandler : IRequestHandler<CreateTaskerProfileCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public CreateTaskerProfileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CreateTaskerProfileCommand request, CancellationToken ct)
        {
            var existingProfile = await _context.TaskerProfiles
                            .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId, ct);

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            // Validator đã bảo đảm Latitude/Longitude không null tới được đây.
            var currentLocation = geometryFactory.CreatePoint(new Coordinate(request.Longitude!.Value, request.Latitude!.Value));

            // Hồ sơ đã bị admin TỪ CHỐI (Status=4) thì cho nộp lại: cập nhật nội dung mới và
            // đưa về hàng chờ duyệt. Không có nhánh này thì thợ bị từ chối sẽ kẹt vĩnh viễn.
            // CHỈ Status=4 mới được nộp lại — Status=2 (bị khóa) KHÔNG, để thợ bị khóa không
            // thể tự thoát trạng thái phạt bằng cách gọi lại API này.
            if (existingProfile != null)
            {
                if (existingProfile.Status != 4)
                    throw new BadRequestException("Tài khoản này đã có hồ sơ thợ.");

                existingProfile.ResubmitForApproval(
                    request.Bio,
                    request.ExperienceYears,
                    currentLocation,
                    request.VerificationImageUrl);

                await _context.SaveChangesAsync(ct);

                return ApiResponse<bool>.Success(true, "Đã nộp lại hồ sơ. Vui lòng chờ quản trị viên phê duyệt.");
            }

            var newProfile = new TaskerProfile
            {
                TaskerProfileId = request.UserId,
                Bio = request.Bio,
                ExperienceYears = request.ExperienceYears,
                CurrentGeom = currentLocation,
                VerificationImageUrl = request.VerificationImageUrl,
                IsVerified = false,
                RatingAvg = 0,
                TotalReviews = 0,
                Status = 0,
                IsDeleted = false
            };

            _context.TaskerProfiles.Add(newProfile);

            // 🗓️ Cấp lịch làm việc MẶC ĐỊNH (T2–T7, 8h–18h) ngay lúc tạo hồ sơ.
            //    Hệ thống chặn đặt lịch ngoài giờ làm việc, nên hồ sơ không có dòng lịch nào
            //    đồng nghĩa KHÔNG AI ĐẶT ĐƯỢC — thợ mới sẽ mãi không có đơn mà không hiểu vì sao.
            //    Xem DefaultWorkingSchedule để biết đầy đủ lý do.
            foreach (var schedule in DefaultWorkingSchedule.For(newProfile.TaskerProfileId))
                _context.TaskerSchedules.Add(schedule);

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Tạo hồ sơ thành công. Vui lòng chờ hệ thống xác minh.");
        }
    }
}
