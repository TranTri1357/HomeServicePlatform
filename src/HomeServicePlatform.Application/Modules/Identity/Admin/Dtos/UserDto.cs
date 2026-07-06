using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Identity.Admin.Dtos
{
    public record UserDto(
        long UserId,
        string FullName,
        string Email,
        string Phone,
        int TotalBookings,    
        decimal TotalSpent,   
        DateTimeOffset CreatedAt, 
        short Status          // (1: Hoạt động, 0: Bị khóa)
    );
}
