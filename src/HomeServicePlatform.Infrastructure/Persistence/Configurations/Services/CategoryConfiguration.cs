using HomeServicePlatform.Domain.Modules.Services.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Services
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> entity)
        {
            entity.ToTable("categories");

            entity.HasKey(e => e.CategoryId);
            entity.Property(e => e.CategoryId).HasColumnName("category_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasColumnName("slug").HasMaxLength(150).IsRequired();
            entity.Property(e => e.IconUrl).HasColumnName("icon_url").HasMaxLength(500);
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(true);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.Slug).IsUnique();
        }
    }
}
