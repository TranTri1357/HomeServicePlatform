using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    public partial class AddSystemWalletsAndLedgerNote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "note",
                table: "wallet_transactions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

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


            migrationBuilder.DropColumn(
                name: "note",
                table: "wallet_transactions");
        }
    }
}
