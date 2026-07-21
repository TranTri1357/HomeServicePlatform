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
        string CategoryName,
        decimal Price,
        int DurationMinutes,
        bool IsActive,
        string? ImageUrl
    );
}
