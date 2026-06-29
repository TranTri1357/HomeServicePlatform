using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Application.Common.Exceptions;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork
{
    public class CompleteWorkCommandHandler : IRequestHandler<CompleteWorkCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        public CompleteWorkCommandHandler(IBookingRepository bookingRepository) => _bookingRepository = bookingRepository;

        public async Task<ApiResponse<bool>> Handle(CompleteWorkCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            try
            {
                booking.CompleteWorkAndPendingPayment(request.TaskerId);
                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Đã gửi hóa đơn dịch vụ, hệ thống chuyển sang trạng thái chờ thanh toán và hoàn thành.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
