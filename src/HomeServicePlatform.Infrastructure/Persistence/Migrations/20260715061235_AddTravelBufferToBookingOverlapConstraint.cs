using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeServicePlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTravelBufferToBookingOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 📌 MIGRATION NO-OP CÓ CHỦ ĐÍCH.
            //
            // Ý định ban đầu: "đệm" ràng buộc EXCLUDE ở CSDL (tstzrange nới ±10 phút) để có sàn cứng
            // buffer di chuyển. NHƯNG cách này SAI THIẾT KẾ: EXCLUDE đệm cố định không phân biệt được
            //   • nhiều dịch vụ trong CÙNG một đơn, CÙNG địa điểm (xếp nối tiếp, buffer đáng lẽ = 0), và
            //   • hai đơn KHÁC địa điểm (cần buffer thật).
            // Đệm cố định sẽ chặn nhầm cả đơn nhiều dịch vụ hợp lệ, và GiST không hỗ trợ toán tử "<>"
            // để loại trừ theo booking_id.
            //
            // => "Thời gian đệm di chuyển" được thực thi hoàn toàn ở TẦNG ỨNG DỤNG (TravelBufferGuard),
            //    tính theo KHOẢNG CÁCH (cùng địa điểm = 0), và an toàn tương tranh nhờ advisory lock
            //    theo thợ (pg_advisory_xact_lock). Ràng buộc chống ĐÈ LỊCH gốc (không đệm) giữ nguyên,
            //    vẫn là lưới an toàn tuyệt đối ở CSDL cho trường hợp trùng giờ thật.
            //
            // Giữ lại migration rỗng này để lịch sử EF nhất quán (đã lỡ scaffold trước đó).
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
