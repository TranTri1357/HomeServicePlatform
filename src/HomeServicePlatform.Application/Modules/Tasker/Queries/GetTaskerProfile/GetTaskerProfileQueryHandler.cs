using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerProfile
{
    public class GetTaskerProfileQueryHandler : IRequestHandler<GetTaskerProfileQuery, ApiResponse<TaskerProfileDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerProfileQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerProfileDto>> Handle(GetTaskerProfileQuery request, CancellationToken cancellationToken)
        {
            // Kết nối tường minh từ TaskerProfiles -> Users (thông qua mối quan hệ 1-1 bằng tasker_profile_id tương ứng user_id)
            var profileQuery = from tp in _context.TaskerProfiles
                               join u in _context.Users on tp.TaskerProfileId equals u.UserId
                               where tp.TaskerProfileId == request.TaskerProfileId && !tp.IsDeleted && !u.IsDeleted
                               select new
                               {
                                   Profile = tp,
                                   User = u,
                                   // Đếm số lượng việc làm dựa trên bảng booking_items với điều kiện status = 1 (Thợ đã nhận/hoàn thành việc)
                                   CompletedJobsCount = _context.BookingItems
                                                                .Count(i => i.TaskerId == tp.TaskerProfileId && i.Status == 1)
                               };

            var data = await profileQuery.FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                throw new NotFoundException($"Không tìm thấy hồ sơ thợ số #{request.TaskerProfileId}");
            }

            var result = new TaskerProfileDto(
                data.Profile.TaskerProfileId,
                data.User.FullName,
                data.User.Phone,
                data.User.Email,
                data.Profile.ExperienceYears,
                data.Profile.RatingAvg,
                data.Profile.TotalReviews,
                data.CompletedJobsCount,
                (short)data.Profile.Status
            );

            return ApiResponse<TaskerProfileDto>.Success(result, "Lấy dữ liệu hồ sơ thợ thành công.");
        }
    }
}
