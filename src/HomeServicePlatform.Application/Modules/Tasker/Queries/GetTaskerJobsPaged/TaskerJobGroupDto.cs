using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobsPaged
{
    public record TaskerJobItemDto(
        long BookingItemId,
        string ServiceName,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt,
        decimal TotalPrice,
        short ItemStatus
    );

    public record TaskerJobGroupDto(
        long BookingId,
        string CustomerName,
        string CustomerPhone,
        string FullAddress,
        short JobStatus,
        DateTimeOffset StartAt,
        decimal Total,
        List<TaskerJobItemDto> Items
    );
}
