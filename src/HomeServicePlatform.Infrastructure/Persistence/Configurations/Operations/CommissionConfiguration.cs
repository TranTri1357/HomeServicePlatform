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
    public class CommissionConfiguration : IEntityTypeConfiguration<Commission>
    {
        public void Configure(EntityTypeBuilder<Commission> entity)
        {
            entity.ToTable("commissions", t =>
            {
                t.HasCheckConstraint("ck_commissions_rate", "commission_rate BETWEEN 0 AND 100");
                t.HasCheckConstraint("ck_commissions_date", "effective_to IS NULL OR effective_from < effective_to");
            });

            entity.HasKey(e => e.CommissionId);
            entity.Property(e => e.CommissionId).HasColumnName("commission_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.CommissionRate).HasColumnName("commission_rate").HasColumnType("decimal(5,2)");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Service).WithMany().HasForeignKey(d => d.ServiceId).HasConstraintName("fk_commissions_service");
            entity.HasOne(d => d.TaskerProfile).WithMany().HasForeignKey(d => d.TaskerId).HasConstraintName("fk_commissions_tasker");

            entity.HasIndex(e => new { e.ServiceId, e.TaskerId }).HasFilter("effective_to IS NULL").HasDatabaseName("ix_commissions_lookup");

        }
    }
}
