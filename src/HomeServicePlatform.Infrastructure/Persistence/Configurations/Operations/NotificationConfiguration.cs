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
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> entity)
        {
            entity.ToTable("notifications", t => t.HasCheckConstraint("ck_notifications_retry", "retry_count >= 0"));

            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.NotificationId).HasColumnName("notification_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Payload).HasColumnName("payload");
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)0);
            entity.Property(e => e.RetryCount).HasColumnName("retry_count").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).HasConstraintName("fk_notifications_user");

            entity.HasIndex(e => new { e.UserId, e.Status }).HasDatabaseName("ix_notifications_user_status");
        }
    }
}
