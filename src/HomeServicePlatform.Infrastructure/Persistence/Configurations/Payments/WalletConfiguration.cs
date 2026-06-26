using HomeServicePlatform.Domain.Modules.Payments.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Infrastructure.Persistence.Configurations.Payments
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> entity)
        {
            entity.ToTable("wallets");

            entity.HasKey(e => e.WalletId);
            entity.Property(e => e.WalletId).HasColumnName("wallet_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Balance).HasColumnName("balance").HasColumnType("decimal(18,2)").HasDefaultValue(0m);

            entity.HasOne(d => d.User).WithOne(p => p.Wallet).HasForeignKey<Wallet>(d => d.UserId).HasConstraintName("fk_wallets_user");
        }
    }
}
