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
    public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
    {
        public void Configure(EntityTypeBuilder<WalletTransaction> entity)
        {
            entity.ToTable("wallet_transactions", t => t.HasCheckConstraint("ck_wallet_transactions_amount", "amount <> 0"));

            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id").UseIdentityByDefaultColumn();

            entity.Property(e => e.WalletId).HasColumnName("wallet_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceBefore).HasColumnName("balance_before").HasColumnType("decimal(18,2)");
            entity.Property(e => e.BalanceAfter).HasColumnName("balance_after").HasColumnType("decimal(18,2)");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.Note).HasColumnName("note").HasMaxLength(200);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Wallet).WithMany(p => p.WalletTransactions).HasForeignKey(d => d.WalletId).HasConstraintName("fk_wallet_transactions_wallet");

            entity.HasIndex(e => new { e.WalletId, e.CreatedAt }).HasDatabaseName("ix_wallet_transactions_wallet_id_created_at");
            entity.HasIndex(e => e.ReferenceId).HasDatabaseName("ix_wallet_transactions_ref");

        }
    }
}
