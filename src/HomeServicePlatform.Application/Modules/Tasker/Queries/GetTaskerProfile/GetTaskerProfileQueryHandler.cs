using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
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
            var profileQuery = from tp in _context.TaskerProfiles
                               join u in _context.Users on tp.TaskerProfileId equals u.UserId
                               where tp.TaskerProfileId == request.TaskerProfileId && !tp.IsDeleted && !u.IsDeleted
                               select new
                               {
                                   Profile = tp,
                                   User = u,
                                   CompletedJobsCount = _context.BookingItems
                                                                .Count(i => i.TaskerId == tp.TaskerProfileId && i.Booking.Status == BookingStatus.Completed)
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
                (short)data.Profile.Status,
                data.Profile.Bio,
                data.Profile.RejectionReason
            );

            return ApiResponse<TaskerProfileDto>.Success(result, "Lấy dữ liệu hồ sơ thợ thành công.");
        }
    }
}
