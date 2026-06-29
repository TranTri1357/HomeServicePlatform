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
        // Khai báo phương thức lấy Aggregate Root kèm nạp dữ liệu liên quan (Include)
        Task<Booking?> GetByIdAsync(long id);

        // Khai báo phương thức lưu/cập nhật toàn bộ khối Aggregate Root
        Task UpdateAggregateAsync(Booking booking);
    }
}
