using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclineEmergencyBooking
{
    public class DeclineEmergencyBookingCommandHandler
        : IRequestHandler<DeclineEmergencyBookingCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeclineEmergencyBookingCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(DeclineEmergencyBookingCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings.AsNoTracking()
                .Where(b => b.BookingId == request.BookingId)
                .Select(b => new { b.BookingId, b.IsEmergency, b.Status })
                .FirstOrDefaultAsync(ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn #{request.BookingId}.");
            if (!booking.IsEmergency)
                throw new BadRequestException("Đây không phải đơn khẩn cấp.");

            // Đơn đã có thợ khác nhận / đã hủy / đã hết hạn: đây là RACE BÌNH THƯỜNG (thợ bấm từ chối
            // đúng lúc người khác vừa nhận), không phải lỗi của thợ này. Trả no-op để client đóng modal
            // êm thay vì hiện thông báo lỗi đỏ.
            if (booking.Status != BookingStatus.Pending)
                return ApiResponse<bool>.Success(true, "Đơn khẩn đã kết thúc.");

            var already = await _context.EmergencyBookingDeclines
                .AnyAsync(d => d.BookingId == request.BookingId && d.TaskerId == request.TaskerId, ct);
            if (already)
                return ApiResponse<bool>.Success(true, "Đã ghi nhận trước đó.");

            _context.EmergencyBookingDeclines.Add(new EmergencyBookingDecline
            {
                BookingId = request.BookingId,
                TaskerId = request.TaskerId,
                WasTimeout = request.WasTimeout,
                CreatedAt = DateTimeOffset.UtcNow
            });

            try
            {
                await _context.SaveChangesAsync(ct);
            }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex))
            {
                // Hai request cùng lúc (bấm từ chối đúng giây hết giờ) — unique index đã chặn bản ghi
                // thừa, đúng như mong muốn. Không phải lỗi.
                return ApiResponse<bool>.Success(true, "Đã ghi nhận trước đó.");
            }

            return ApiResponse<bool>.Success(true, "Đã bỏ qua đơn khẩn cấp.");
        }

        // Postgres unique_violation = SQLSTATE 23505 (cùng lối nhận diện với IsExclusionViolation
        // trong AcceptBookingCommandHandler, tránh phụ thuộc cứng vào kiểu của Npgsql).
        private static bool IsUniqueViolation(Exception ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner) as string;
                if (sqlState == "23505") return true;
            }
            return false;
        }
    }
}
