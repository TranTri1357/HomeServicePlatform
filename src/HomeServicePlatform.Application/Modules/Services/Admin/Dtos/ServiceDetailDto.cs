using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Dtos
{
    public record ServiceDetailDto(
        long ServiceId,
        int CategoryId,
        string Name,
        string? Description,
        int DurationMinutes,
        bool IsActive,
        string? ImageUrl
    );
}
