using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Bookings
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> entity)
        {
            entity.ToTable("bookings", t => t.HasCheckConstraint("ck_bookings_amount", "subtotal_amount >= 0 AND discount_amount >= 0 AND final_amount >= 0"));

            entity.HasKey(e => e.BookingId);
            entity.Property(e => e.BookingId).HasColumnName("booking_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue(BookingStatus.Pending); ;
            entity.Property(e => e.SubtotalAmount).HasColumnName("subtotal_amount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.DiscountAmount).HasColumnName("discount_amount").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(e => e.FinalAmount).HasColumnName("final_amount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(1000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RowVersion).HasColumnName("row_version").HasDefaultValue(1).IsConcurrencyToken();

            entity.Property(e => e.IsEmergency).HasColumnName("is_emergency").HasDefaultValue(false);
            entity.Property(e => e.EmergencyExpiresAt).HasColumnName("emergency_expires_at");

            entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId).HasConstraintName("fk_bookings_customer");

            entity.HasIndex(e => e.CustomerId).HasDatabaseName("ix_bookings_customer_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_bookings_status");

        }
    }
}
