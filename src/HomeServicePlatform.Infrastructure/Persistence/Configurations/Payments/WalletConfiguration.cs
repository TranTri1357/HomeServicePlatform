using HomeServicePlatform.Domain.Modules.Payments.Constants;
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

            // 🔒 Chống "lost update" trên số dư: mọi thao tác ví đều là đọc-sửa-ghi, nếu hai giao
            // dịch chạy song song trên cùng một ví (đặc biệt là ví ký quỹ — nơi MỌI đơn đều đi qua)
            // thì bản ghi sau sẽ đè mất bản ghi trước và tiền bị sai. Dùng cột hệ thống "xmin" của
            // PostgreSQL làm concurrency token: không phát sinh cột mới, xung đột sẽ ném
            // DbUpdateConcurrencyException thay vì âm thầm nuốt mất tiền.
            // "xmin" là cột hệ thống CÓ SẴN của PostgreSQL (số hiệu transaction đã ghi hàng đó),
            // nên đây chỉ là khai báo ánh xạ, không phải cột mới. Lưu ý: EF vẫn sinh lệnh
            // AddColumn("xmin") khi scaffold — lệnh đó đã được gỡ thủ công khỏi migration
            // AddSystemWalletsAndLedgerNote, vì ADD COLUMN lên cột hệ thống sẽ lỗi.
            entity.Property<uint>("xmin").IsRowVersion();

            SeedSystemWallets(entity);
        }

        /// <summary>
        /// Hai ví hệ thống điều phối dòng tiền của sàn (chủ sở hữu là user hệ thống được seed ở
        /// <c>UserConfiguration</c>):
        ///   • Ví KÝ QUỸ   — giữ hộ tiền đã thu của đơn chưa tất toán.
        ///   • Ví DOANH THU — hoa hồng đã chốt và phí hủy sàn giữ lại.
        /// Seed khóa chính cứng để không phụ thuộc thứ tự cấp phát identity.
        /// </summary>
        private static void SeedSystemWallets(EntityTypeBuilder<Wallet> entity)
        {
            entity.HasData(
                new Wallet
                {
                    WalletId = SystemAccounts.EscrowWalletId,
                    UserId = SystemAccounts.EscrowUserId,
                    Balance = 0m
                },
                new Wallet
                {
                    WalletId = SystemAccounts.RevenueWalletId,
                    UserId = SystemAccounts.RevenueUserId,
                    Balance = 0m
                });
        }
    }
}
