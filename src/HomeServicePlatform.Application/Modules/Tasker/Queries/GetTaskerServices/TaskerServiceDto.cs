using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerServices
{
    public record TaskerServiceDto(
        long TaskerServiceId,
        long ServiceId,
        string ServiceName,
        string CategoryName,      // Loại dịch vụ (Danh mục)
        decimal Price,            // Giá tiền riêng của thợ cho dịch vụ này
        int DurationMinutes,      // Thời gian thực hiện (Phút)
        bool IsActive,
        string? ImageUrl          // Ảnh dịch vụ (Cloudinary); null thì UI hiện icon mặc định
    );
}
