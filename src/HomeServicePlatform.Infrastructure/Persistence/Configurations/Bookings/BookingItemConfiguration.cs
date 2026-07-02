using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Bookings
{
    public class BookingItemConfiguration : IEntityTypeConfiguration<BookingItem>
    {
        public void Configure(EntityTypeBuilder<BookingItem> entity)
        {
            entity.ToTable("booking_items", t =>
            {
                t.HasCheckConstraint("ck_booking_items_quantity", "quantity > 0");
                t.HasCheckConstraint("ck_booking_items_duration", "duration_minutes > 0");
                t.HasCheckConstraint("ck_booking_items_unit_price", "unit_price >= 0");
                t.HasCheckConstraint("ck_booking_items_total_price", "total_price >= 0");
                t.HasCheckConstraint("ck_booking_items_time", "start_at < end_at");
            });

            entity.HasKey(e => e.BookingItemId);
            entity.Property(e => e.BookingItemId).HasColumnName("booking_item_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnName("total_price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)0);
            entity.Property(e => e.CancelRejectReason).HasColumnName("cancel_reject_reason").HasMaxLength(500);
            entity.Property(e => e.RowVersion).HasColumnName("row_version").HasDefaultValue(1).IsConcurrencyToken();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingItems).HasForeignKey(d => d.BookingId).HasConstraintName("fk_booking_items_booking");
            entity.HasOne(d => d.Service).WithMany(p => p.BookingItems).HasForeignKey(d => d.ServiceId).HasConstraintName("fk_booking_items_service");
            entity.HasOne(d => d.TaskerProfile).WithMany().HasForeignKey(d => d.TaskerId).HasConstraintName("fk_booking_items_tasker_profile");

            entity.HasIndex(e => new { e.TaskerId, e.StartAt, e.EndAt }).HasDatabaseName("ix_booking_items_tasker_time");
            entity.HasIndex(e => e.BookingId).HasDatabaseName("ix_booking_items_booking_id");
            entity.HasIndex(e => e.ServiceId).HasDatabaseName("ix_booking_items_service_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_booking_items_status");

        }
    }
}
