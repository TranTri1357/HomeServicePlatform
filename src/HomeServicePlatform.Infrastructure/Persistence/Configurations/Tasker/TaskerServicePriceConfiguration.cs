using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Tasker
{
    public class TaskerServicePriceConfiguration : IEntityTypeConfiguration<TaskerServicePrice>
    {
        public void Configure(EntityTypeBuilder<TaskerServicePrice> entity)
        {
            entity.ToTable("tasker_service_prices", t =>
            {
                t.HasCheckConstraint("ck_service_prices_price", "price >= 0");
                t.HasCheckConstraint("ck_service_prices_date", "effective_to IS NULL OR effective_from < effective_to");
            });

            entity.HasKey(e => e.TaskerServicePriceId);
            entity.Property(e => e.TaskerServicePriceId).HasColumnName("tasker_service_price_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(18,2)");
            entity.Property(e => e.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(e => e.EffectiveTo).HasColumnName("effective_to");

            entity.HasOne(d => d.Service).WithMany(p => p.TaskerServicePrices).HasForeignKey(d => d.ServiceId).HasConstraintName("fk_service_prices_service");
            entity.HasOne(d => d.TaskerProfile).WithMany().HasForeignKey(d => d.TaskerId).HasConstraintName("fk_service_prices_tasker");

            entity.HasIndex(e => new { e.ServiceId, e.EffectiveFrom, e.EffectiveTo }).HasDatabaseName("ix_service_prices_service_id_effective");

        }
    }
}
