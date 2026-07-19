using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking;
using HomeServicePlatform.Application.Modules.Booking.Emergency;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.RebroadcastEmergencyBooking
{
    public class RebroadcastEmergencyBookingCommandHandler
        : IRequestHandler<RebroadcastEmergencyBookingCommand, ApiResponse<CreateEmergencyBookingResponse>>
    {
        private const double MaxRadiusKm = 15.0;
        private static readonly TimeSpan ResponseWindow = TimeSpan.FromSeconds(30);

        private readonly IApplicationDbContext _context;

        public RebroadcastEmergencyBookingCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CreateEmergencyBookingResponse>> Handle(RebroadcastEmergencyBookingCommand request, CancellationToken ct)
        {
            if (request.RadiusKm <= 0 || request.RadiusKm > MaxRadiusKm)
                throw new BadRequestException($"Bán kính không hợp lệ (tối đa {MaxRadiusKm:0}km).");

            var booking = await _context.Bookings
                .Include(b => b.BookingItems)
                .Include(b => b.BookingAddress)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn #{request.BookingId}.");
            if (!booking.IsEmergency)
                throw new BadRequestException("Đây không phải đơn khẩn cấp.");
            if (booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền thao tác đơn này.");
            if (booking.Status != BookingStatus.Pending)
                throw new BadRequestException("Đơn khẩn không còn ở trạng thái chờ (đã có thợ nhận hoặc đã hủy).");
            if (booking.BookingAddress?.Geom == null)
                throw new BadRequestException("Đơn khẩn thiếu tọa độ, không thể quét thợ.");

            // Gia hạn cửa sổ phản hồi cho vòng mới.
            var nowUtc = DateTimeOffset.UtcNow;
            booking.EmergencyExpiresAt = nowUtc.Add(ResponseWindow);
            booking.UpdatedAt = nowUtc;
            await _context.SaveChangesAsync(ct);

            var serviceId = booking.BookingItems.Select(i => i.ServiceId).First();
            var serviceName = await _context.Services
                .AsNoTracking()
                .Where(s => s.ServiceId == serviceId)
                .Select(s => s.Name)
                .FirstOrDefaultAsync(ct) ?? "dịch vụ";

            var lat = booking.BookingAddress.Geom.Y;
            var lng = booking.BookingAddress.Geom.X;

            // Thợ đã CHỦ ĐỘNG bấm "Từ chối" ở vòng trước thì không mời lại — vòng sau bán kính rộng
            // hơn nên tập thợ là tập cha, không lọc thì họ bị dựng dậy lại cho cùng một đơn.
            // Thợ hết 30s không phản hồi (WasTimeout) VẪN được mời lại: có thể lúc đó họ đang bận tay.
            var declinedTaskerIds = await _context.EmergencyBookingDeclines
                .AsNoTracking()
                .Where(d => d.BookingId == booking.BookingId && !d.WasTimeout)
                .Select(d => d.TaskerId)
                .ToListAsync(ct);

            var taskers = await EmergencyTaskerFinder.FindEligibleAsync(
                _context, serviceId, lat, lng, request.RadiusKm, ct, declinedTaskerIds);

            var response = new CreateEmergencyBookingResponse(
                booking.BookingId,
                serviceName,
                booking.BookingAddress.AddressLine ?? string.Empty,
                lat,
                lng,
                (int)ResponseWindow.TotalSeconds,
                request.RadiusKm,
                taskers);

            return ApiResponse<CreateEmergencyBookingResponse>.Success(response, "Đã nới bán kính tìm thợ.");
        }
    }
}
