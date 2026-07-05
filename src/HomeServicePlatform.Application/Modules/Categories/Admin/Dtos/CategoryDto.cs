using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Dtos
{
    public record CategoryDto(
        int CategoryId,
        string? IconUrl,
        string Name,
        int TotalServices,   
        int TotalTaskers,   
        int TotalBookings,   
        bool IsActive
    );
}
