using System;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetBookingDetail
{
    public record BookingDetailDto(
        long BookingId,
        string CustomerName,      // Tên khách hàng đặt đơn
        string ContactName,       // Tên người nhận trên đơn
        string ContactPhone,      // SĐT liên hệ trên đơn
        string? TaskerName,       // Tên thợ thực hiện (null nếu chưa có thợ)
        short Status,             // Trạng thái đơn (0..6)
        decimal SubtotalAmount,   // Giá gốc trước giảm
        decimal DiscountAmount,   // Số tiền giảm
        decimal FinalAmount,      // Số tiền khách phải trả
        string? Note,             // Ghi chú của khách
        DateTimeOffset CreatedAt, // Ngày tạo đơn
        string FullAddress        // Địa chỉ thực hiện dịch vụ
    );
}
