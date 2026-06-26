using HomeServicePlatform.Domain.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Identity
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> entity)
        {
            entity.ToTable("users", t => t.HasCheckConstraint("ck_users_status", "status IN (0, 1, 2)"));

            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Phone).HasColumnName("phone").HasMaxLength(20).IsRequired();
            entity.Property(e => e.FullName).HasColumnName("full_name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(255).IsRequired();
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)1);
            entity.Property(e => e.LastLoginAt).HasColumnName("last_login_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.RowVersion).HasColumnName("row_version").HasDefaultValue(1).IsConcurrencyToken();
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

            entity.HasIndex(e => e.Email).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ux_users_email");
            entity.HasIndex(e => e.Phone).IsUnique().HasFilter("is_deleted = false").HasDatabaseName("ux_users_phone");
            entity.HasIndex(e => e.Status).HasDatabaseName("ix_users_status");

        }
    }
}
