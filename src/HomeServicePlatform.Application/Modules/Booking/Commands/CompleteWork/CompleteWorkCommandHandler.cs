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

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork
{
    public class CompleteWorkCommandHandler : IRequestHandler<CompleteWorkCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;

        public CompleteWorkCommandHandler(IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CompleteWorkCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            try
            {
                booking.CompleteWorkAndPendingPayment(request.TaskerId);

                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.WorkCompleted,
                    "Hoàn thành công việc",
                    $"Đơn BK{booking.BookingId} đã hoàn thành. Vui lòng thanh toán và đánh giá thợ."));

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Đã gửi hóa đơn dịch vụ, hệ thống chuyển sang trạng thái chờ thanh toán và hoàn thành.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
