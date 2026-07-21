using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Bookings
{
    public class EmergencyBookingDeclineConfiguration : IEntityTypeConfiguration<EmergencyBookingDecline>
    {
        public void Configure(EntityTypeBuilder<EmergencyBookingDecline> entity)
        {
            entity.ToTable("emergency_booking_declines");

            entity.HasKey(e => e.EmergencyBookingDeclineId);
            entity.Property(e => e.EmergencyBookingDeclineId)
                .HasColumnName("emergency_booking_decline_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.WasTimeout).HasColumnName("was_timeout").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.EmergencyDeclines)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_emergency_booking_declines_booking");

            entity.HasOne<Domain.Modules.Tasker.Entities.TaskerProfile>()
                .WithMany()
                .HasForeignKey(d => d.TaskerId)
                .HasConstraintName("fk_emergency_booking_declines_tasker_profile");

            entity.HasIndex(e => new { e.BookingId, e.TaskerId })
                .IsUnique()
                .HasDatabaseName("ux_emergency_booking_declines_booking_tasker");
        }
    }
}
