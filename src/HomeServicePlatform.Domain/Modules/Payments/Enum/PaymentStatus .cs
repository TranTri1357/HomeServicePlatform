using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Payments.Enum
{
    public enum PaymentStatus : short
    {
        Pending = 1,
        Paid = 2,
        Failed = 3,
        Cancelled = 4,
        Refunded = 5
    }
}
