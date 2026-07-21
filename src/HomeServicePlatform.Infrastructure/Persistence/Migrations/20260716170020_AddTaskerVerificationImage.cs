using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    public partial class AddTaskerVerificationImage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rejection_reason",
                table: "tasker_profile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "verification_image_url",
                table: "tasker_profile",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rejection_reason",
                table: "tasker_profile");

            migrationBuilder.DropColumn(
                name: "verification_image_url",
                table: "tasker_profile");
        }
    }
}
