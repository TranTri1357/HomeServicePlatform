using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using Microsoft.EntityFrameworkCore;
using DomainBooking = HomeServicePlatform.Domain.Modules.Bookings.Entities.Booking;
using DomainBookingAddress = HomeServicePlatform.Domain.Modules.Bookings.Entities.BookingAddress;
using DomainBookingItem = HomeServicePlatform.Domain.Modules.Bookings.Entities.BookingItem;


namespace HomeServicePlatform.Infrastructure.Persistence.Repositories.Bookings
{
    public class BookingRepository : IBookingRepository
    {
        private readonly ApplicationDbContext _context;

        public BookingRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SaveAggregateAsync(DomainBooking booking)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<DomainBooking?> GetByIdAsync(long id)
        {
            // Dùng Include để nạp sẵn dữ liệu BookingItems và BookingHistories lên bộ nhớ RAM.
            // Tránh lỗi Lazy Loading và chuẩn hóa dữ liệu cho Aggregate Root xử lý logic.
            return await _context.Bookings
                .Include(b => b.BookingItems)
                .Include(b => b.BookingHistories)
                .FirstOrDefaultAsync(b => b.BookingId == id);
        }

        public async Task UpdateAggregateAsync(DomainBooking booking)
        {
            // Sử dụng Database Transaction đảm bảo chuỗi cập nhật (bảng chính + bảng phụ) 
            // Nếu có 1 lệnh lỗi, toàn bộ dữ liệu sẽ tự động rollback an toàn tuyệt đối.
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // ⚠️ KHÔNG gọi _context.Bookings.Update(booking): booking được nạp ở chế
                // độ tracking (GetByIdAsync), còn Update() đánh dấu CẢ graph là Modified —
                // khiến BookingHistory mới (HistoryId = 0) bị hiểu nhầm là "cập nhật" →
                // sinh UPDATE 0 dòng, im lặng không INSERT (bug mất lịch sử chuyển trạng thái).
                // Chỉ cần SaveChanges: ChangeTracker tự nhận diện bản ghi mới trong collection
                // là Added (INSERT) và các thay đổi trạng thái là Modified (UPDATE).
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; // Ném ngược lỗi ra ngoài để Global Middleware xử lý
            }
        }
    }
}
