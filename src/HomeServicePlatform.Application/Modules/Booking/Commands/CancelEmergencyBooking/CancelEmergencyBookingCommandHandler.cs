using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Emergency;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelEmergencyBooking
{
    public class CancelEmergencyBookingCommandHandler
        : IRequestHandler<CancelEmergencyBookingCommand, ApiResponse<EmergencyCancelResult>>
    {
        // Vòng quét rộng nhất mà frontend có thể đã nới tới (RADII = 5/10/15km). Khi hủy một đơn
        // broadcast ta quét lại ở bán kính này để phủ HẾT những thợ có thể đang mở modal.
        private const double MaxBroadcastRadiusKm = 15.0;

        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;

        public CancelEmergencyBookingCommandHandler(
            IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
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

            var notifyTaskerIds = await ResolveTaskersToNotifyAsync(booking, taskerId, ct);

            return ApiResponse<EmergencyCancelResult>.Success(
                new EmergencyCancelResult(booking.CustomerId, taskerId, notifyTaskerIds),
                "Đã hủy đơn khẩn cấp.");
        }

        /// <summary>
        /// Ai cần được báo "đơn đã hủy" để đóng modal đang kêu chuông?
        ///  • Đơn đã gán thợ → đúng thợ đó.
        ///  • Đơn BROADCAST (chưa ai nhận, taskerId = 0) → không có cột nào lưu danh sách đã bắn,
        ///    nên quét lại thợ đủ điều kiện ở bán kính rộng nhất (15km). Đây là TẬP CHA của những
        ///    thợ đã nhận broadcast, nên không sót ai; thợ thừa nhận được cũng vô hại vì client bỏ
        ///    qua nếu bookingId không khớp modal đang mở.
        /// </summary>
        private async Task<IReadOnlyList<long>> ResolveTaskersToNotifyAsync(
            Domain.Modules.Bookings.Entities.Booking booking, long assignedTaskerId, CancellationToken ct)
        {
            if (assignedTaskerId != 0) return new[] { assignedTaskerId };

            var serviceId = booking.BookingItems.Select(i => i.ServiceId).FirstOrDefault();
            if (serviceId == 0) return Array.Empty<long>();

            // GetByIdAsync không nạp sẵn BookingAddress → lấy tọa độ bằng truy vấn riêng.
            var point = await _context.BookingAddresses.AsNoTracking()
                .Where(a => a.BookingId == booking.BookingId && a.Geom != null)
                .Select(a => new { Lat = a.Geom!.Y, Lng = a.Geom.X })
                .FirstOrDefaultAsync(ct);
            if (point == null) return Array.Empty<long>();

            // Thợ đã bấm từ chối thì modal của họ đã đóng — không cần báo nữa.
            var declinedTaskerIds = await _context.EmergencyBookingDeclines
                .AsNoTracking()
                .Where(d => d.BookingId == booking.BookingId && !d.WasTimeout)
                .Select(d => d.TaskerId)
                .ToListAsync(ct);

            var taskers = await EmergencyTaskerFinder.FindEligibleAsync(
                _context, serviceId, point.Lat, point.Lng, MaxBroadcastRadiusKm, ct, declinedTaskerIds);

            return taskers.Select(t => t.TaskerId).ToList();
        }
    }
}
