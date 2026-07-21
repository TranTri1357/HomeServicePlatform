using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        long? UserId { get; }

        string? Email { get; }

        List<string> Roles { get; }

        bool IsAuthenticated { get; }
    }
}
