using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Application.Common.Exceptions;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking
{
    public class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        public AcceptBookingCommandHandler(IBookingRepository bookingRepository) => _bookingRepository = bookingRepository;

        public async Task<ApiResponse<bool>> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            try
            {
                booking.AcceptByTasker(request.TaskerId);
                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Thợ đã xác nhận nhận lịch làm việc.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
