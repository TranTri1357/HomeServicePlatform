using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Reviews.Admin.Commands.DeleteReview
{
    public class DeleteReviewCommandHandler : IRequestHandler<DeleteReviewCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public DeleteReviewCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(DeleteReviewCommand request, CancellationToken ct)
        {
            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == request.ReviewId && !r.IsDeleted, ct);

            if (review == null)
                throw new NotFoundException("Không tìm thấy đánh giá hoặc đánh giá đã bị gỡ.");

            review.IsDeleted = true;
            review.UpdatedAt = DateTimeOffset.UtcNow;

            var remainingRatings = await _context.Reviews
                .Where(r => r.TaskerId == review.TaskerId && !r.IsDeleted && r.ReviewId != review.ReviewId)
                .Select(r => (int)r.Rating)
                .ToListAsync(ct);

            var tasker = await _context.TaskerProfiles
                .FirstOrDefaultAsync(t => t.TaskerProfileId == review.TaskerId, ct);
            if (tasker != null)
            {
                tasker.TotalReviews = remainingRatings.Count;
                tasker.RatingAvg = remainingRatings.Count > 0
                    ? Math.Round((decimal)remainingRatings.Sum() / remainingRatings.Count, 1)
                    : 0m;
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Đã gỡ đánh giá thành công.");
        }
    }
}
