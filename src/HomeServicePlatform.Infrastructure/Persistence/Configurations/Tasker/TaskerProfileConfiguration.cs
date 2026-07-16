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
    public class TaskerProfileConfiguration : IEntityTypeConfiguration<TaskerProfile>
    {
        public void Configure(EntityTypeBuilder<TaskerProfile> entity)
        {
            entity.ToTable("tasker_profile", t =>
            {
                t.HasCheckConstraint("ck_tasker_profile_rating", "rating_avg BETWEEN 0 AND 5");
                t.HasCheckConstraint("ck_tasker_profile_experience", "experience_years >= 0");
            });

            entity.HasKey(e => e.TaskerProfileId);
            entity.Property(e => e.TaskerProfileId).HasColumnName("tasker_profile_id");

            entity.Property(e => e.Bio).HasColumnName("bio").HasMaxLength(1000);
            entity.Property(e => e.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
            entity.Property(e => e.VerifiedAt).HasColumnName("verified_at");
            entity.Property(e => e.ExperienceYears).HasColumnName("experience_years").HasDefaultValue(0);
            entity.Property(e => e.CurrentGeom).HasColumnName("current_geom");
            entity.Property(e => e.VerificationImageUrl).HasColumnName("verification_image_url").HasMaxLength(500);
            entity.Property(e => e.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.Property(e => e.RatingAvg).HasColumnName("rating_avg").HasColumnType("decimal(3,2)").HasDefaultValue(0m);
            entity.Property(e => e.TotalReviews).HasColumnName("total_reviews").HasDefaultValue(0);
            entity.Property(e => e.Status).HasColumnName("status").HasDefaultValue((short)0);
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);
            entity.Property(e => e.CancelCount).HasColumnName("cancel_count").HasDefaultValue(0);
            entity.Property(e => e.CompletedCount).HasColumnName("completed_count").HasDefaultValue(0);

            entity.HasOne(d => d.User).WithOne(p => p.TaskerProfile).HasForeignKey<TaskerProfile>(d => d.TaskerProfileId).HasConstraintName("fk_tasker_profile_user");

            entity.HasIndex(e => e.Status).HasDatabaseName("ix_tasker_profile_status");
            entity.HasIndex(e => e.IsVerified).HasDatabaseName("ix_tasker_profile_verified");
            entity.HasIndex(e => e.RatingAvg).HasDatabaseName("ix_tasker_profile_rating");
            entity.HasIndex(e => e.CurrentGeom).HasMethod("gist").HasDatabaseName("gist_tasker_current_geom");
        }
    }
}
