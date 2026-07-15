using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Customer.Entities;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Services.Entities;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        // Khai báo các bảng dữ liệu bạn cần dùng ở tầng Application
        DbSet<User> Users { get; set; }
        DbSet<Role> Roles { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<Token> Tokens { get; set; }
        DbSet<Address> Addresses { get; set; }
        DbSet<TaskerProfile> TaskerProfiles { get; set; }
        DbSet<Category> Categories { get; set; }
        DbSet<Service> Services { get; set; }
        DbSet<TaskerService> TaskerServices { get; set; }
        DbSet<TaskerServicePrice> TaskerServicePrices { get; set; }
        DbSet<TaskerSchedule> TaskerSchedules { get; set; }
        DbSet<TaskerTimeOff> TaskerTimeOffs { get; set; }
        DbSet<Booking> Bookings { get; set; }
        DbSet<BookingAddress> BookingAddresses { get; set; }
        DbSet<BookingItem> BookingItems { get; set; }
        DbSet<Payment> Payments { get; set; }
        DbSet<Refund> Refunds { get; set; }
        DbSet<Wallet> Wallets { get; set; }
        DbSet<WalletTransaction> WalletTransactions { get; set; }
        DbSet<Commission> Commissions { get; set; }
        DbSet<BookingHistory> BookingHistories { get; set; }
        DbSet<Review> Reviews { get; set; }
        DbSet<Notification> Notifications { get; set; }
        DbSet<Dispute> Disputes { get; set; }
        DbSet<Message> Messages { get; set; }

        // Bắt buộc phải có hàm này để luồng Query có thể gọi CancellationToken nếu cần
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Giữ advisory lock theo thợ (pg_advisory_xact_lock) trong transaction hiện tại — dùng để
        /// tuần tự hóa việc đặt lịch của cùng một thợ, chống double-booking khi có nhiều request đồng thời.
        /// Khóa tự nhả khi transaction kết thúc (commit/rollback). Phải gọi bên trong một transaction.
        /// </summary>
        Task AcquireTaskerScheduleLockAsync(long taskerId, CancellationToken cancellationToken = default);
    }
}
