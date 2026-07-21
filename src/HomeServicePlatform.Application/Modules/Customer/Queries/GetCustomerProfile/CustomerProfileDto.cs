using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Customer.Queries.GetCustomerProfile
{
    public record CustomerProfileDto(
        long CustomerId,
        string FullName,
        string Phone,
        string Email,
        string DefaultAddress,
        int TotalBookingsCount,
        int CompletedBookingsCount
    );
}
