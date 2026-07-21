using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    public partial class AddEmergencyBookingDeclines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emergency_booking_declines",
                columns: table => new
                {
                    emergency_booking_decline_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    tasker_id = table.Column<long>(type: "bigint", nullable: false),
                    was_timeout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emergency_booking_declines", x => x.emergency_booking_decline_id);
                    table.ForeignKey(
                        name: "fk_emergency_booking_declines_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "booking_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_emergency_booking_declines_tasker_profile",
                        column: x => x.tasker_id,
                        principalTable: "tasker_profile",
                        principalColumn: "tasker_profile_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_emergency_booking_declines_tasker_id",
                table: "emergency_booking_declines",
                column: "tasker_id");

            migrationBuilder.CreateIndex(
                name: "ux_emergency_booking_declines_booking_tasker",
                table: "emergency_booking_declines",
                columns: new[] { "booking_id", "tasker_id" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emergency_booking_declines");
        }
    }
}
