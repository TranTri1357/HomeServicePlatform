using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    /// <summary>
    /// Một hạng mục dịch vụ trong đơn. Một đơn (Booking) có thể chứa nhiều hạng mục.
    /// </summary>
    public record MyBookingItemDto(
        long BookingItemId,
        string ServiceName,
        long? TaskerId,
        string? TaskerName,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        int Quantity,
        decimal UnitPrice,
        decimal TotalPrice,
        short Status,
        // Khách đã đánh giá hạng mục này chưa (để ẩn nút "Đánh giá").
        bool HasReview
    );

    /// <summary>
    /// Một đơn đặt lịch của khách, kèm đầy đủ danh sách hạng mục và các cờ trạng thái
    /// dùng để bật/tắt nút Thanh toán, Đánh giá, Khiếu nại ở giao diện.
    /// </summary>
    public record MyBookingDto(
        long BookingId,
        string FullAddress,
        decimal SubtotalAmount,
        decimal? DiscountAmount,
        decimal FinalAmount,
        string? Note,
        DateTimeOffset CreatedAt,
        short Status,
        // Đơn đã có giao dịch thanh toán thành công chưa (để ẩn nút "Thanh toán").
        bool IsPaid,
        // Đơn đã có khiếu nại chưa (để ẩn nút "Khiếu nại").
        bool HasDispute,
        List<MyBookingItemDto> Items
    );
}
