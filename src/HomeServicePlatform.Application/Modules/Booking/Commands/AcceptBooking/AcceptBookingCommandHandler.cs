using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
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

            // 🚨 Đơn khẩn cấp: quá 30s (EmergencyExpiresAt) thì không cho nhận nữa.
            if (booking.IsEmergency && booking.EmergencyExpiresAt.HasValue
                && DateTimeOffset.UtcNow > booking.EmergencyExpiresAt.Value)
            {
                throw new BadRequestException("Đơn khẩn cấp đã hết hạn 30 giây, không thể nhận.");
            }

            try
            {
                booking.AcceptByTasker(request.TaskerId);

                // 🔔 Thông báo cho khách (cùng transaction với UpdateAggregateAsync).
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.BookingAccepted,
                    "Thợ đã nhận đơn",
                    $"Đơn BK{booking.BookingId} đã được thợ tiếp nhận."));

                // 🚨 Đơn khẩn cấp thanh toán tiền mặt sau: tạo Payment tiền mặt (Pending) khi thợ
                // nhận, để đơn lọt "settled predicate" và hiện trong danh sách việc của thợ.
                if (booking.IsEmergency)
                {
                    _context.Payments.Add(new Payment
                    {
                        BookingId = booking.BookingId,
                        Amount = booking.FinalAmount,
                        Method = (short)PaymentMethod.Cash,
                        Status = (short)PaymentStatus.Pending,
                        CreatedAt = DateTimeOffset.UtcNow,
                        RowVersion = 1
                    });
                }

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Thợ đã xác nhận nhận lịch làm việc.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }
    }
}
