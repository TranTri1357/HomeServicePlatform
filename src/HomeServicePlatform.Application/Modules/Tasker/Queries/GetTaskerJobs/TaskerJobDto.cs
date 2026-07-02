using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobs
{
    public record TaskerJobDto(
        long BookingItemId,     // ID của hạng mục công việc cụ thể
        long BookingId,         // ID của đơn hàng tổng
        string ServiceName,     // Tên dịch vụ cần làm
        string CustomerName,    // Tên khách hàng đặt lịch
        string CustomerPhone,   // Số điện thoại khách hàng
        DateTimeOffset StartAt, // Thời gian bắt đầu làm
        DateTimeOffset EndAt,   // Thời gian kết thúc ca làm
        string FullAddress,     // Địa điểm đến làm việc
        decimal TotalPrice,     // Số tiền thợ nhận được cho việc này
        short JobStatus         // Trạng thái công việc của thợ
    );
}
