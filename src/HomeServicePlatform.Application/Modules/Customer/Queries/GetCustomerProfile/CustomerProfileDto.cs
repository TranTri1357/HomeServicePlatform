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
        int TotalBookingsCount,     // Số đơn đã đặt
        int CompletedBookingsCount  // Số đơn đã hoàn thành (Status = 2 hoặc tùy theo cấu hình DB của bạn)
    );
}
