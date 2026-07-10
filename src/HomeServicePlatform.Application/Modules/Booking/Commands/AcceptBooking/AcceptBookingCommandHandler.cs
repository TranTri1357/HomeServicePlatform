using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking
{
    public class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;

        public AcceptBookingCommandHandler(IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            try
            {
                booking.AcceptByTasker(request.TaskerId);

                // 🔔 Thông báo cho khách (cùng transaction với UpdateAggregateAsync).
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.BookingAccepted,
                    "Thợ đã nhận đơn",
                    $"Đơn BK{booking.BookingId} đã được thợ tiếp nhận."));

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Thợ đã xác nhận nhận lịch làm việc.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
