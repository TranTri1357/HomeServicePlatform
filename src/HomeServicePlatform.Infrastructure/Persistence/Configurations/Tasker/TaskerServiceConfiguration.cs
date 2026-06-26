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
    public class TaskerServiceConfiguration : IEntityTypeConfiguration<TaskerService>
    {
        public void Configure(EntityTypeBuilder<TaskerService> entity)
        {
            entity.ToTable("tasker_services");
            entity.HasKey(e => new { e.TaskerId, e.ServiceId });

            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");

            entity.HasOne(d => d.TaskerProfile).WithMany(p => p.TaskerServices).HasForeignKey(d => d.TaskerId).HasConstraintName("fk_tasker_services_tasker_profile");
            entity.HasOne(d => d.Service).WithMany(p => p.TaskerServices).HasForeignKey(d => d.ServiceId).HasConstraintName("fk_tasker_services_service");

            entity.HasIndex(e => e.TaskerId).HasDatabaseName("ix_tasker_services_tasker_id");
            entity.HasIndex(e => e.ServiceId).HasDatabaseName("ix_tasker_services_service_id");
        }
    }
}
