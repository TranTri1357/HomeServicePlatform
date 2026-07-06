using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Dtos
{
    public record UserDetailDto(
        long UserId,
        string FullName,
        string Email,
        string Phone,
        short Status,
        DateTimeOffset CreatedAt,
        DateTimeOffset? LastLoginAt,
        decimal WalletBalance,              
        List<string> Roles,                 
        List<UserAddressDto> Addresses     
    );
}
