using System;
using System.Collections.Generic;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerAvailability
{
    public record AvailabilitySlotDto(TimeOnly Time, bool IsFree);

    public record TaskerAvailabilityDto(
        DateOnly Date,
        bool HasSchedule,
        List<AvailabilitySlotDto> Slots
    );
}
