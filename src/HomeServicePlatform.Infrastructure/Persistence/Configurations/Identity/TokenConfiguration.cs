using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Domain.Modules.Identity.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Identity
{
    public class TokenConfiguration : IEntityTypeConfiguration<Token>
    {
        public void Configure(EntityTypeBuilder<Token> entity)
        {
            entity.ToTable("tokens");

            entity.HasKey(e => e.TokenId);
            entity.Property(e => e.TokenId).HasColumnName("token_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.TokenString).HasColumnName("token").HasMaxLength(512).IsRequired();
            entity.Property(e => e.ExpiredAt).HasColumnName("expired_at");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.IsRevoked).HasColumnName("is_revoked").HasDefaultValue(false).IsRequired();
            entity.Property(e => e.RevokedAt).HasColumnName("revoked_at").IsRequired(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.User).WithMany().HasForeignKey(d => d.UserId).HasConstraintName("fk_tokens_user");

            entity.HasIndex(e => e.UserId).HasDatabaseName("ix_tokens_user_id");
            entity.HasIndex(e => e.ExpiredAt).HasDatabaseName("ix_tokens_expired_at");
        }
    }
}
