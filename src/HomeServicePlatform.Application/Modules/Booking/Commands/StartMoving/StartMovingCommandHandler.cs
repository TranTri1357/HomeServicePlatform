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

namespace HomeServicePlatform.Application.Modules.Booking.Commands.StartMoving
{
    public class StartMovingCommandHandler : IRequestHandler<StartMovingCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;

        public StartMovingCommandHandler(IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(StartMovingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            try
            {
                booking.StartMovingByTasker(request.TaskerId);

                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.TaskerOnTheWay,
                    "Thợ đang đến",
                    $"Thợ đang trên đường đến chỗ bạn (đơn BK{booking.BookingId})."));

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Hệ thống ghi nhận thợ đang trên đường di chuyển.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
