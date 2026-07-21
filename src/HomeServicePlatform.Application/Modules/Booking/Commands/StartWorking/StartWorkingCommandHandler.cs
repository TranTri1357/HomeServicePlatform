using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.StartWorking
{
    public class StartWorkingCommandHandler : IRequestHandler<StartWorkingCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;

        public StartWorkingCommandHandler(IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(StartWorkingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            bool hasAnotherInProgress = await _context.BookingItems.AnyAsync(bi =>
                bi.TaskerId == request.TaskerId
                && bi.BookingId != request.BookingId
                && bi.Status == (short)BookingStatus.InProgress, cancellationToken);

            if (hasAnotherInProgress)
                throw new BadRequestException("Bạn đang thực hiện một công việc khác. Hãy hoàn thành nó trước khi bắt đầu việc mới.");

            try
            {
                booking.StartWorkingByTasker(request.TaskerId);

                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.WorkStarted,
                    "Bắt đầu thực hiện",
                    $"Thợ đã bắt đầu làm việc cho đơn BK{booking.BookingId}."));

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Dịch vụ đã chính thức bắt đầu triển khai.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
