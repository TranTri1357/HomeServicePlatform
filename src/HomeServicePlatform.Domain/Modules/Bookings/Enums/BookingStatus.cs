using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Bookings.Enums
{
    public enum BookingStatus : short
    {
        Pending = 0,          // Chờ xác nhận (Khách vừa đặt đơn)
        Accepted = 1,         // Đã xác nhận (Thợ bấm nhận đơn)
        OnTheWay = 2,         // Trên đường đến (Thợ bấm di chuyển tới nhà khách)
        InProgress = 3,       // Đang thực hiện (Thợ bắt đầu làm việc) 
        Completed = 4,        // Hoàn thành (Tiền đã vào ví, kết thúc đơn hoàn toàn)
        Cancelled = 5,         // Đã hủy (Khách hoặc hệ thống hủy đơn)
        Refund = 6           // Đang hoàn tiền (Admin đang xử lý hoàn tiền cho khách)
    }
}
