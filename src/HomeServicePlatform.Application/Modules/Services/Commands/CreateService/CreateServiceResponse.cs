using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Commands.CreateService
{
    public record CreateServiceResponse(
        long ServiceId,
        string Name,
        DateTimeOffset CreatedAt
    );
}
