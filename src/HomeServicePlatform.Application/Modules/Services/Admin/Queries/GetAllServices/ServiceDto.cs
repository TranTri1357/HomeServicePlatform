using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetAllServices
{
    public record ServiceDto(long ServiceId, string Name, string? Description, int DurationMinutes, bool IsActive);
}
