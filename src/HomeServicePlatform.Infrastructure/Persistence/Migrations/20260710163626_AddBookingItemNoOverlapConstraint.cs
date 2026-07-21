using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    public partial class AddBookingItemNoOverlapConstraint : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql(@"
                ALTER TABLE booking_items
                ADD CONSTRAINT ex_booking_items_no_overlap
                EXCLUDE USING gist (
                    tasker_id WITH =,
                    tstzrange(start_at, end_at) WITH &&
                )
                WHERE (status <> 5 AND tasker_id IS NOT NULL);
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE booking_items DROP CONSTRAINT IF EXISTS ex_booking_items_no_overlap;");
        }
    }
}
