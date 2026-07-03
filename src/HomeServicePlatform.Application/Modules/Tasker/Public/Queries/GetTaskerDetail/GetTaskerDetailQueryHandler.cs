using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerDetail
{
    public class GetTaskerDetailQueryHandler : IRequestHandler<GetTaskerDetailQuery, ApiResponse<TaskerDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerDetailDto>> Handle(GetTaskerDetailQuery request, CancellationToken ct)
        {
            var tasker = await _context.TaskerProfiles
                .AsNoTracking()
                .Where(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted)
                .Select(t => new TaskerDetailDto
                {
                    TaskerId = t.TaskerProfileId,
                    FullName = t.User.FullName,
                    AvatarUrl = null,
                    Bio = t.Bio,
                    IsVerified = t.IsVerified,
                    ExperienceYears = t.ExperienceYears,
                    RatingAvg = t.RatingAvg,
                    TotalReviews = t.TotalReviews,

                    TotalJobs = _context.BookingItems
                                .Count(bi => bi.TaskerId == t.TaskerProfileId &&
                                             bi.Booking.Status == BookingStatus.Completed),

                    Skills = t.TaskerServices
                              .Where(ts => ts.Service.IsActive && !ts.Service.IsDeleted)
                              .Select(ts => ts.Service.Name)
                              .ToList(),

                    Certificates = new List<string>()
                })
                .FirstOrDefaultAsync(ct);

            if (tasker == null)
                throw new NotFoundException($"Không tìm thấy hồ sơ thợ với ID {request.TaskerId}.");

            var reviewsQuery = _context.Reviews
                .AsNoTracking()
                .Where(r => r.TaskerId == request.TaskerId && !r.IsDeleted);

            var starGroup = await reviewsQuery
                .GroupBy(r => r.Rating)
                .Select(g => new { Rating = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.Rating, v => v.Count, ct);

            tasker.ReviewSummary = new ReviewSummaryDto
            {
                FiveStarCount = starGroup.GetValueOrDefault((short)5, 0),
                FourStarCount = starGroup.GetValueOrDefault((short)4, 0),
                ThreeStarCount = starGroup.GetValueOrDefault((short)3, 0),
                TwoStarCount = starGroup.GetValueOrDefault((short)2, 0),
                OneStarCount = starGroup.GetValueOrDefault((short)1, 0)
            };

            tasker.RecentReviews = await reviewsQuery
                .OrderByDescending(r => r.CreatedAt)
                .Take(5)
                .Select(r => new TaskerReviewDto
                {
                    ReviewId = r.ReviewId,
                    CustomerName = r.Customer.FullName,
                    CustomerAvatarUrl = null,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(ct);

            return ApiResponse<TaskerDetailDto>.Success(tasker, "Lấy thông tin thợ thành công.");
        }
    }
}
