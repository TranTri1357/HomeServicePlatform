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
    public class TaskerTimeOffConfiguration : IEntityTypeConfiguration<TaskerTimeOff>
    {
        public void Configure(EntityTypeBuilder<TaskerTimeOff> entity)
        {
            entity.ToTable("tasker_time_offs", t => t.HasCheckConstraint("ck_tasker_time_offs_time", "start_at < end_at"));

            entity.HasKey(e => e.TimeOffId);
            entity.Property(e => e.TimeOffId).HasColumnName("time_off_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.StartAt).HasColumnName("start_at");
            entity.Property(e => e.EndAt).HasColumnName("end_at");
            entity.Property(e => e.Reason).HasColumnName("reason").HasMaxLength(500);

            entity.HasOne(d => d.TaskerProfile).WithMany(p => p.TaskerTimeOffs).HasForeignKey(d => d.TaskerId).HasConstraintName("fk_tasker_time_offs_tasker_profile");
        }
    }
}
