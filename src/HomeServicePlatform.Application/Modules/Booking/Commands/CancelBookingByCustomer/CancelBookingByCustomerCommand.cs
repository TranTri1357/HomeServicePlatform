using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBookingByCustomer
{
    public record CancelBookingByCustomerCommand(
        long BookingId,
        long CustomerId,
        string CancelReason
    ) : IRequest<ApiResponse<bool>>;
}
