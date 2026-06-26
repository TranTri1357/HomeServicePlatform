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
    public class TaskerScheduleConfiguration : IEntityTypeConfiguration<TaskerSchedule>
    {
        public void Configure(EntityTypeBuilder<TaskerSchedule> entity)
        {
            entity.ToTable("tasker_schedules", t =>
            {
                t.HasCheckConstraint("ck_tasker_schedules_day", "day_of_week BETWEEN 0 AND 6");
                t.HasCheckConstraint("ck_tasker_schedules_time", "start_time < end_time");
            });

            entity.HasKey(e => e.ScheduleId);
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.StartTime).HasColumnName("start_time").HasColumnType("time");
            entity.Property(e => e.EndTime).HasColumnName("end_time").HasColumnType("time");

            entity.HasOne(d => d.TaskerProfile).WithMany(p => p.TaskerSchedules).HasForeignKey(d => d.TaskerId).HasConstraintName("fk_tasker_schedules_tasker_profile");

        }
    }
}
