using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelEmergencyBooking
{
    public class CancelEmergencyBookingCommandHandler
        : IRequestHandler<CancelEmergencyBookingCommand, ApiResponse<EmergencyCancelResult>>
    {
        private readonly IBookingRepository _bookingRepository;

        public CancelEmergencyBookingCommandHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

        public async Task<ApiResponse<EmergencyCancelResult>> Handle(CancelEmergencyBookingCommand request, CancellationToken ct)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn #{request.BookingId}.");
            if (!booking.IsEmergency)
                throw new BadRequestException("Đây không phải đơn khẩn cấp.");
            if (booking.Status != BookingStatus.Pending)
                throw new BadRequestException("Đơn khẩn không còn ở trạng thái chờ nên không thể hủy.");

            var taskerId = booking.BookingItems.Select(i => i.TaskerId).FirstOrDefault(id => id != null) ?? 0;

            // Chỉ chủ đơn hoặc thợ được gán mới được hủy.
            if (request.ActorUserId != booking.CustomerId && request.ActorUserId != taskerId)
                throw new ForbiddenException("Bạn không có quyền hủy đơn này.");

            var oldStatus = booking.Status;
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTime.UtcNow;
            foreach (var item in booking.BookingItems)
            {
                item.Status = (short)BookingStatus.Cancelled;
                item.UpdatedAt = DateTime.UtcNow;
            }
            booking.BookingHistories.Add(new BookingHistory
            {
                BookingId = booking.BookingId,
                OldStatus = (short)oldStatus,
                NewStatus = (short)BookingStatus.Cancelled,
                ChangedBy = request.ActorUserId,
                CreatedAt = DateTime.UtcNow
            });

            await _bookingRepository.UpdateAggregateAsync(booking);

            return ApiResponse<EmergencyCancelResult>.Success(
                new EmergencyCancelResult(booking.CustomerId, taskerId),
                "Đã hủy đơn khẩn cấp.");
        }
    }
}
