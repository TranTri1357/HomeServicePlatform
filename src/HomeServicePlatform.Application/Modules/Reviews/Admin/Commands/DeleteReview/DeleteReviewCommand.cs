using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Reviews.Admin.Commands.DeleteReview
{
    public record DeleteReviewCommand(long ReviewId) : IRequest<ApiResponse<bool>>;
}
