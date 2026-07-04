using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings
{
    // Flat DTO: Chỉ chứa những cột giao diện cần hiển thị, phẳng hóa dữ liệu để DB truy vấn siêu tốc
    public record BookingLookupDto(
        long BookingId,
        string CustomerName,
        string CustomerPhone,
        decimal FinalAmount,
        short Status,
        DateTimeOffset CreatedAt,
        string FullAddress
    );

    // Sử dụng Record gộp tham số đầu vào cho API GET phân trang
    public record GetAllBookingsQuery(
        string? SearchTerm,
        short? Status,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<BookingLookupDto>>>;
}
