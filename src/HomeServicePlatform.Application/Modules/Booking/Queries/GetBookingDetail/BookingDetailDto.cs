using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetBookingDetail
{
    public record BookingDetailDto(
        long BookingId,
        string CustomerName,      // Tên khách hàng đặt đơn
        string? TaskerName,       // Tên thợ thực hiện (Có thể null nếu đơn chưa có thợ)
        short Status,             // Trạng thái đơn hàng (0: Pending, 1: Accepted,...)
        decimal SubtotalAmount,   // Giá gốc trước giảm
        decimal DiscountAmount,   // Số tiền giảm giá
        decimal FinalAmount,       // Số tiền thực tế khách phải trả
        DateTime CreatedAt,       // Ngày thực hiện tạo đơn (Múi giờ UTC)
        string FullAddress        // Địa chỉ thực hiện dịch vụ
    );
}
