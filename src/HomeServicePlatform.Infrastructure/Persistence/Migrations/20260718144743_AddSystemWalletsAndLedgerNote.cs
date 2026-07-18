using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemWalletsAndLedgerNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ⚠️ CỐ Ý KHÔNG tạo cột "xmin" cho bảng wallets.
            //    "xmin" là cột hệ thống có sẵn của PostgreSQL và được dùng làm concurrency token
            //    cho ví (chống lost update khi hai giao dịch cùng sửa một số dư). EF scaffold ra
            //    lệnh AddColumn cho nó, nhưng chạy thật sẽ lỗi vì không thể ADD COLUMN trùng tên
            //    cột hệ thống — nên lệnh đó đã được gỡ. Model snapshot vẫn giữ khai báo là đúng.

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "wallet_transactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            // ⚠️ Cột "status" được THÊM TAY vào danh sách dưới đây.
            //    UserConfiguration seed Status = 0 (khóa), nhưng 0 đúng bằng giá trị mặc định của
            //    kiểu short nên EF coi như "không gán" và bỏ hẳn cột khỏi lệnh INSERT. Cột lại có
            //    HasDefaultValue((short)1) => hai tài khoản hệ thống sẽ nằm ở trạng thái ĐANG HOẠT
            //    ĐỘNG, ngược hẳn với ý định. Ghi rõ giá trị ở đây để lấp lại lớp bảo vệ đó.
            //    (Không sửa ModelSnapshot: snapshot và model đang khớp nhau ở chỗ cùng bỏ qua cột
            //     này, thêm vào một bên sẽ khiến lần scaffold sau sinh ra thay đổi giả.)
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "created_at", "email", "full_name", "is_deleted", "last_login_at", "password_hash", "phone", "row_version", "status", "updated_at" },
                values: new object[,]
                {
                    { 9000000001L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "escrow@system.local", "Ví ký quỹ hệ thống", true, null, "$2a$11$SystemAccountNoLogin00abcdefghijklmnopqrstuvwxyz01234", "SYSTEM-ESCROW", 1, (short)0, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) },
                    { 9000000002L, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "revenue@system.local", "Ví doanh thu hệ thống", true, null, "$2a$11$SystemAccountNoLogin00abcdefghijklmnopqrstuvwxyz01234", "SYSTEM-REVENUE", 1, (short)0, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) }
                });

            migrationBuilder.InsertData(
                table: "wallets",
                columns: new[] { "wallet_id", "user_id" },
                values: new object[,]
                {
                    { 9000000001L, 9000000001L },
                    { 9000000002L, 9000000002L }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "wallets",
                keyColumn: "wallet_id",
                keyValue: 9000000001L);

            migrationBuilder.DeleteData(
                table: "wallets",
                keyColumn: "wallet_id",
                keyValue: 9000000002L);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: 9000000001L);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: 9000000002L);

            // Không DropColumn "xmin" — xem ghi chú ở Up(): cột này chưa từng được tạo.

            migrationBuilder.DropColumn(
                name: "note",
                table: "wallet_transactions");
        }
    }
}
