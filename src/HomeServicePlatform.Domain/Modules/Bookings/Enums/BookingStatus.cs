using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Domain.Modules.Bookings.Enums
{
    public enum BookingStatus : short
    {
        Pending = 0,
        Accepted = 1,
        OnTheWay = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5,
        Refund = 6,
        DisputeRejected = 7
    }
}
