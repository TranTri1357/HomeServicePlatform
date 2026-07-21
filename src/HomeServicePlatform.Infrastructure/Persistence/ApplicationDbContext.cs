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
    public class ApplicationDbContext : DbContext, IApplicationDbContext
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
        public DbSet<EmergencyBookingDecline> EmergencyBookingDeclines { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Refund> Refunds { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }
        public DbSet<Commission> Commissions { get; set; }
        public DbSet<BookingHistory> BookingHistories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<Message> Messages { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasPostgresExtension("postgis");
            modelBuilder.HasPostgresExtension("btree_gist");
            modelBuilder.HasPostgresExtension("pg_trgm");

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                var now = DateTimeOffset.UtcNow;

                if (entry.State == EntityState.Modified)
                {
                    var updatedAtProp = entry.Entity.GetType().GetProperty("UpdatedAt");
                    if (updatedAtProp != null && updatedAtProp.CanWrite)
                    {
                        updatedAtProp.SetValue(entry.Entity, now);
                    }
                }

                if (entry.State == EntityState.Added)
                {
                    var createdAtProp = entry.Entity.GetType().GetProperty("CreatedAt");
                    if (createdAtProp != null && createdAtProp.CanWrite)
                    {
                        createdAtProp.SetValue(entry.Entity, now);
                    }

                    var updatedAtProp = entry.Entity.GetType().GetProperty("UpdatedAt");
                    if (updatedAtProp != null && updatedAtProp.CanWrite)
                    {
                        updatedAtProp.SetValue(entry.Entity, now);
                    }
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }

        public Task AcquireTaskerScheduleLockAsync(long taskerId, CancellationToken cancellationToken = default)
            => Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock({taskerId})", cancellationToken);

        public Task AcquireBookingClaimLockAsync(long bookingId, CancellationToken cancellationToken = default)
            => Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(1, {(int)bookingId})", cancellationToken);

        public void ResetTrackedChanges() => ChangeTracker.Clear();
    }
}
