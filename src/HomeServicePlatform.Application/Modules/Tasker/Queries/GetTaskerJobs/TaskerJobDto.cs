using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobs
{
    public record TaskerJobDto(
        long BookingItemId,
        long BookingId,
        string ServiceName,
        string CustomerName,
        string CustomerPhone,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        string FullAddress,
        decimal TotalPrice,
        short JobStatus
    );
}
