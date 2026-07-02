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
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> entity)
        {
            entity.ToTable("payments", t => t.HasCheckConstraint("ck_payments_amount", "amount >= 0"));

            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.PaymentId).HasColumnName("payment_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingId).HasColumnName("booking_id");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.Method).HasColumnName("method");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)0);
            entity.Property(e => e.TransactionCode).HasColumnName("transaction_code").HasMaxLength(100);
            entity.Property(e => e.PaidAt).HasColumnName("paid_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RowVersion).HasColumnName("row_version").HasDefaultValue(1).IsConcurrencyToken();

            entity.HasOne(d => d.Booking).WithMany(p => p.Payments).HasForeignKey(d => d.BookingId).HasConstraintName("fk_payments_booking");

            entity.HasIndex(e => e.BookingId).HasDatabaseName("ix_payments_booking_id");
        }
    }
}
