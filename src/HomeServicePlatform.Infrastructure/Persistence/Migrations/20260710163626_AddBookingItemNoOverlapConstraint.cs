using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingItemNoOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Cần btree_gist để dùng toán tử "=" cho bigint trong EXCLUDE constraint.
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // 🛡️ CHỐNG ĐẶT TRÙNG LỊCH THỢ (đúng yêu cầu lõi đồ án):
            // Không cho hai booking_item của cùng một thợ có khoảng thời gian [start_at, end_at)
            // đè lên nhau — trừ đơn đã hủy (status = 5). Postgres tự chặn ở tầng CSDL,
            // an toàn tuyệt đối kể cả khi hai request insert đồng thời.
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE booking_items DROP CONSTRAINT IF EXISTS ex_booking_items_no_overlap;");
        }
    }
}
