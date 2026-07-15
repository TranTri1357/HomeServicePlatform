using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Dtos
{
    public record ServiceDto(
        long ServiceId,
        string Name,
        string CategoryName,
        int TotalTaskers,
        int TotalBookings,
        bool IsActive,
        string? ImageUrl
    );
}
