using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    /// <summary>
    /// Danh sách đơn của khách, LỌC + PHÂN TRANG ở server để không tải toàn bộ.
    /// </summary>
    /// <param name="Statuses">Lọc theo tập trạng thái (theo tab). Rỗng = tất cả.</param>
    /// <param name="SearchTerm">Tìm theo mã đơn (BK123 / 123) hoặc tên dịch vụ.</param>
    public record GetMyBookingsQuery(
        long CustomerId,
        IReadOnlyList<short>? Statuses = null,
        string? SearchTerm = null,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<MyBookingDto>>>;
}
