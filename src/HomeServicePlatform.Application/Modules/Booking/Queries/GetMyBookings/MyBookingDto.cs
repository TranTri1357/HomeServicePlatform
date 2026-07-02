using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    public record MyBookingDto(
        long BookingId,
        string ServiceName,
        long? TaskerId,
        string? TaskerName,
        DateTimeOffset StartAt, // 🟢 Sửa từ DateTime thành DateTimeOffset
        DateTimeOffset EndAt,
        string FullAddress,
        decimal FinalAmount,
        short Status
    );
}
