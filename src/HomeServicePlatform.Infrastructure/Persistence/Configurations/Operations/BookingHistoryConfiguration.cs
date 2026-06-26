using HomeServicePlatform.Domain.Modules.Operations.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Operations
{
    public class BookingHistoryConfiguration : IEntityTypeConfiguration<BookingHistory>
    {
        public void Configure(EntityTypeBuilder<BookingHistory> entity)
        {
            entity.ToTable("booking_histories");

            entity.HasKey(e => e.HistoryId);
            entity.Property(e => e.HistoryId).HasColumnName("history_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.OldStatus).HasColumnName("old_status");
            entity.Property(e => e.NewStatus).HasColumnName("new_status");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Booking).WithMany(p => p.BookingHistories).HasForeignKey(d => d.BookingId).HasConstraintName("fk_booking_histories_booking");
            entity.HasOne(d => d.Changer).WithMany().HasForeignKey(d => d.ChangedBy).HasConstraintName("fk_booking_histories_changed_by");

            entity.HasIndex(e => e.BookingId).HasDatabaseName("ix_booking_histories_booking_id");
        }
    }
}
