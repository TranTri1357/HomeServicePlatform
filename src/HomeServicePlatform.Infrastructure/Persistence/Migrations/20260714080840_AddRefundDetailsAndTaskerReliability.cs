using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    public partial class AddRefundDetailsAndTaskerReliability : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cancel_count",
                table: "tasker_profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "completed_count",
                table: "tasker_profile",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<short>(
                name: "status",
                table: "refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)0);

            migrationBuilder.AddColumn<long>(
                name: "booking_id",
                table: "refunds",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "completed_at",
                table: "refunds",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "initiated_by",
                table: "refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "reason",
                table: "refunds",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "refund_method",
                table: "refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.CreateIndex(
                name: "ix_refunds_booking_id",
                table: "refunds",
                column: "booking_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_refunds_booking_id",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "cancel_count",
                table: "tasker_profile");

            migrationBuilder.DropColumn(
                name: "completed_count",
                table: "tasker_profile");

            migrationBuilder.DropColumn(
                name: "booking_id",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "completed_at",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "initiated_by",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "reason",
                table: "refunds");

            migrationBuilder.DropColumn(
                name: "refund_method",
                table: "refunds");

            migrationBuilder.AlterColumn<short>(
                name: "status",
                table: "refunds",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                oldClrType: typeof(short),
                oldType: "smallint",
                oldDefaultValue: (short)1);
        }
    }
}
