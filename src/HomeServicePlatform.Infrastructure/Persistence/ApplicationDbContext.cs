using HomeServicePlatform.Application.Common.Interfaces;
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
    public class ApplicationDbContext :DbContext, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        #region DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Token> Tokens { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<TaskerProfile> TaskerProfiles { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<TaskerService> TaskerServices { get; set; }
        public DbSet<TaskerServicePrice> TaskerServicePrices { get; set; }
        public DbSet<TaskerSchedule> TaskerSchedules { get; set; }
        public DbSet<TaskerTimeOff> TaskerTimeOffs { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingAddress> BookingAddresses { get; set; }
        public DbSet<BookingItem> BookingItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<BookingHistory> BookingHistories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
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
