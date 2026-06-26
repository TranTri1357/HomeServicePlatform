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
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> entity)
        {
            entity.ToTable("reviews", t => t.HasCheckConstraint("ck_reviews_rating", "rating BETWEEN 1 AND 5"));

            entity.HasKey(e => e.ReviewId);
            entity.Property(e => e.ReviewId).HasColumnName("review_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.BookingItemId).HasColumnName("booking_item_id");
            entity.Property(e => e.TaskerId).HasColumnName("tasker_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Rating).HasColumnName("rating");
            entity.Property(e => e.Comment).HasColumnName("comment").HasMaxLength(2000);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

            entity.HasOne(d => d.BookingItem).WithOne().HasForeignKey<Review>(d => d.BookingItemId).HasConstraintName("fk_reviews_booking_item");
            entity.HasOne(d => d.TaskerProfile).WithMany().HasForeignKey(d => d.TaskerId).HasConstraintName("fk_reviews_tasker_profile");
            entity.HasOne(d => d.Customer).WithMany().HasForeignKey(d => d.CustomerId).HasConstraintName("fk_reviews_customer");

            entity.HasIndex(e => e.Rating).HasDatabaseName("ix_reviews_rating");
            entity.HasIndex(e => e.TaskerId).HasDatabaseName("ix_reviews_tasker_id");
            entity.HasIndex(e => e.CustomerId).HasDatabaseName("ix_reviews_customer_id");

        }
    }
}
