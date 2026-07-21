using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview
{
    public record GetCancellationPreviewQuery(
        long BookingId,
        long CustomerId
    ) : IRequest<ApiResponse<CancellationPreviewDto>>;
}
