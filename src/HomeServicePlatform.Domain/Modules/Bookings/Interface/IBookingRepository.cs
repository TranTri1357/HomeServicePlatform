using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;

namespace HomeServicePlatform.Domain.Modules.Bookings.Interface
{
    public interface IBookingRepository
    {
        Task SaveAggregateAsync(Booking booking);
        Task<Booking?> GetByIdAsync(long id);

        Task UpdateAggregateAsync(Booking booking);
    }
}
