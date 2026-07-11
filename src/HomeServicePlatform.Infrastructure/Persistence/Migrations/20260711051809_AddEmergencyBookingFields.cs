using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyBookingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "emergency_expires_at",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_emergency",
                table: "bookings",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "emergency_expires_at",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "is_emergency",
                table: "bookings");
        }
    }
}
