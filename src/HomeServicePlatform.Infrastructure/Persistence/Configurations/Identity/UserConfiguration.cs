using HomeServicePlatform.Domain.Modules.Identity.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
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

            SeedSystemAccounts(entity);
        }

        /// <summary>
        /// Hai user "kỹ thuật" làm chủ sở hữu của ví ký quỹ và ví doanh thu (bảng wallets có
        /// khóa ngoại 1-1 sang users nên ví hệ thống buộc phải có chủ).
        ///
        /// 🛡️ Không thể đăng nhập, theo ba lớp:
        ///   1. IsDeleted = true  -> truy vấn đăng nhập lọc "!IsDeleted" nên không bao giờ tìm thấy
        ///      (đồng thời ẩn khỏi mọi danh sách người dùng và khỏi unique index lọc is_deleted).
        ///   2. Status = 0        -> trạng thái khóa.
        ///   3. PasswordHash là chuỗi BCrypt đúng định dạng nhưng không ứng với mật khẩu nào.
        /// Hai tài khoản này cũng không được gán UserRole nào nên không mang quyền gì.
        /// </summary>
        private static void SeedSystemAccounts(EntityTypeBuilder<User> entity)
        {
            // Mốc thời gian cố định: HasData yêu cầu giá trị tất định, nếu dùng DateTimeOffset.UtcNow
            // thì mỗi lần scaffold migration sẽ sinh ra thay đổi giả.
            var seededAt = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

            // BCrypt hợp lệ về cú pháp (cost 11 + salt 22 ký tự + hash 31 ký tự) nhưng không phải
            // hash của mật khẩu nào -> Verify trả về false thay vì ném lỗi phân tích salt.
            const string unusableHash = "$2a$11$SystemAccountNoLogin00abcdefghijklmnopqrstuvwxyz01234";

            entity.HasData(
                new User
                {
                    UserId = SystemAccounts.EscrowUserId,
                    Email = "escrow@system.local",
                    Phone = "SYSTEM-ESCROW",
                    FullName = "Ví ký quỹ hệ thống",
                    PasswordHash = unusableHash,
                    Status = 0,
                    CreatedAt = seededAt,
                    UpdatedAt = seededAt,
                    RowVersion = 1,
                    IsDeleted = true
                },
                new User
                {
                    UserId = SystemAccounts.RevenueUserId,
                    Email = "revenue@system.local",
                    Phone = "SYSTEM-REVENUE",
                    FullName = "Ví doanh thu hệ thống",
                    PasswordHash = unusableHash,
                    Status = 0,
                    CreatedAt = seededAt,
                    UpdatedAt = seededAt,
                    RowVersion = 1,
                    IsDeleted = true
                });
        }
    }
}
