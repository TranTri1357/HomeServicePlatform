using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Reviews.Admin.Commands.DeleteReview
{
    /// <summary>Admin gỡ (soft-delete) một đánh giá không phù hợp.</summary>
    public record DeleteReviewCommand(long ReviewId) : IRequest<ApiResponse<bool>>;
}
