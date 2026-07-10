using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability
{
    public record AvailabilitySlotDto(TimeOnly Time, bool IsFree);

    /// <summary>Khung giờ làm việc của thợ trong một ngày (free/busy) cho khách chọn lịch.</summary>
    public record TaskerAvailabilityDto(
        DateOnly Date,
        bool HasSchedule,                    // Thợ có đặt lịch làm việc ngày này không
        List<AvailabilitySlotDto> Slots
    );
}
