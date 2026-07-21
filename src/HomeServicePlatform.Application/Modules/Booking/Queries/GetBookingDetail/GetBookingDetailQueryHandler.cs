using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetBookingDetail
{
    public class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, ApiResponse<BookingDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetBookingDetailQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<BookingDetailDto>> Handle(GetBookingDetailQuery request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Customer)
                .Include(b => b.BookingAddress)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy dữ liệu chi tiết cho đơn hàng số #{request.BookingId}");

            var firstTaskerId = await _context.BookingItems
                .AsNoTracking()
                .Where(i => i.BookingId == request.BookingId && i.TaskerId != null)
                .OrderBy(i => i.BookingItemId)
                .Select(i => i.TaskerId)
                .FirstOrDefaultAsync(ct);

            string? taskerName = null;
            if (firstTaskerId != null)
            {
                taskerName = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.UserId == firstTaskerId)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync(ct);
            }

            var addr = booking.BookingAddress;

            var dto = new BookingDetailDto(
                booking.BookingId,
                booking.Customer.FullName,
                addr?.FullName ?? booking.Customer.FullName,
                addr?.Phone ?? "",
                taskerName,
                (short)booking.Status,
                booking.SubtotalAmount,
                booking.DiscountAmount ?? 0m,
                booking.FinalAmount,
                booking.Note,
                booking.CreatedAt,
                addr != null ? $"{addr.AddressLine}, {addr.WardCode}" : "Chưa cập nhật địa chỉ");

            return ApiResponse<BookingDetailDto>.Success(dto, "Tải dữ liệu đơn hàng thành công.");
        }
    }
}
