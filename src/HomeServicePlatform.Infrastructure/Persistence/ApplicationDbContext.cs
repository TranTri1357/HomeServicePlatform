using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Customer.Entities;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Services.Entities;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Infrastructure.Persistence
{
    public class ApplicationDbContext :DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        #region DbSets
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Token> Tokens => Set<Token>();
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<TaskerProfile> TaskerProfiles => Set<TaskerProfile>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Service> Services => Set<Service>();
        public DbSet<TaskerService> TaskerServices => Set<TaskerService>();
        public DbSet<TaskerServicePrice> TaskerServicePrices => Set<TaskerServicePrice>();
        public DbSet<TaskerSchedule> TaskerSchedules => Set<TaskerSchedule>();
        public DbSet<TaskerTimeOff> TaskerTimeOffs => Set<TaskerTimeOff>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingAddress> BookingAddresses => Set<BookingAddress>();
        public DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<Refund> Refunds => Set<Refund>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
        public DbSet<Commission> Commissions => Set<Commission>();
        public DbSet<BookingHistory> BookingHistories => Set<BookingHistory>();
        public DbSet<Review> Reviews => Set<Review>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<Dispute> Disputes => Set<Dispute>();
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Kích hoạt Extension hệ thống
            modelBuilder.HasPostgresExtension("postgis");
            modelBuilder.HasPostgresExtension("btree_gist");

            // TỰ ĐỘNG NẠP TOÀN BỘ CONFIGURATION FILE (Quét qua Assembly hiện tại)
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
