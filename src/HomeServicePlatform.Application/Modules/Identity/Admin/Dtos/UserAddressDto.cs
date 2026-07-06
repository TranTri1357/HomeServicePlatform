using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Dtos
{
    public record UserAddressDto(
        long AddressId,
        string AddressLine,
        bool IsDefault
    );
}
