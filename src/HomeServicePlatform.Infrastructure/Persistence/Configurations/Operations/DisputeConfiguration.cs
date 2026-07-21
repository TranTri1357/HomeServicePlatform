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
    public class DisputeConfiguration : IEntityTypeConfiguration<Dispute>
    {
        public void Configure(EntityTypeBuilder<Dispute> entity)
        {
            entity.ToTable("disputes", t => t.HasCheckConstraint("ck_disputes_refund", "refund_amount >= 0"));

            entity.HasKey(e => e.DisputeId);
            entity.Property(e => e.DisputeId).HasColumnName("dispute_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.RaisedById).HasColumnName("raised_by_id");
            entity.Property(e => e.Reason).HasColumnName("reason").IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)0);
            entity.Property(e => e.ResolutionNote).HasColumnName("resolution_note");
            entity.Property(e => e.RefundAmount).HasColumnName("refund_amount").HasColumnType("decimal(18,2)").HasDefaultValue(0m);
            entity.Property(e => e.ResolvedAt).HasColumnName("resolved_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RowVersion).HasColumnName("row_version").HasDefaultValue(1).IsConcurrencyToken();

            entity.HasOne(d => d.Booking).WithMany(p => p.Disputes).HasForeignKey(d => d.BookingId).HasConstraintName("fk_disputes_booking");

            entity.HasOne(d => d.RaisedBy).WithMany().HasForeignKey(d => d.RaisedById).OnDelete(DeleteBehavior.Restrict).HasConstraintName("fk_disputes_raised_by");

            entity.HasIndex(e => e.BookingId).HasDatabaseName("ix_disputes_booking_id");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_disputes_status");

        }
    }
}
