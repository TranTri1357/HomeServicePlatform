using HomeServicePlatform.Domain.Modules.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Payments
{
    public class RefundConfiguration : IEntityTypeConfiguration<Refund>
    {
        public void Configure(EntityTypeBuilder<Refund> entity)
        {
            entity.ToTable("refunds", t => t.HasCheckConstraint("ck_refunds_amount", "amount >= 0"));

            entity.HasKey(e => e.RefundId);
            entity.Property(e => e.RefundId).HasColumnName("refund_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)1);
            entity.Property(e => e.InitiatedBy).HasColumnName("initiated_by").HasDefaultValue((short)0);
            entity.Property(e => e.RefundMethod).HasColumnName("refund_method").HasDefaultValue((short)0);
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");

            entity.HasOne(d => d.Payment).WithMany(p => p.Refunds).HasForeignKey(d => d.PaymentId).HasConstraintName("fk_refunds_payment");

            entity.HasIndex(e => e.PaymentId).HasDatabaseName("ix_refunds_payment_id");
            entity.HasIndex(e => e.BookingId).HasDatabaseName("ix_refunds_booking_id");
        }
    }
}
