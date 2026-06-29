using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking
{
    public record CreateBookingResponse(
        long BookingId,
        string Message,
        bool IsSuccess,
        decimal FinalAmount
    );
}
