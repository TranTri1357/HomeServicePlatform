using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Categories.Admin.Dtos
{
    public record CategoryDetailDto(int CategoryId, string Name, string Slug, string? IconUrl, bool IsActive);
}
