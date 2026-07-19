using System;
using System.Linq.Expressions;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Định nghĩa DUY NHẤT cho câu hỏi "hạng mục này có đang chiếm khung giờ của thợ không?".
    ///
    /// Điều kiện ở đây phải khớp ĐÚNG ràng buộc <c>ex_booking_items_no_overlap</c> của CSDL
    /// (chặn mọi trạng thái trừ Hủy), cộng thêm quy tắc TTL cho đơn mới giữ chỗ:
    ///   • Hủy(5)                      → nhả chỗ cho người khác đặt.
    ///   • Chờ xác nhận(0)             → chỉ giữ chỗ trong hạn TTL; quá hạn coi như bỏ dở.
    ///   • Đã nhận / đang làm(1,2,3)   → thợ đang bận.
    ///   • Hoàn thành(4), Hoàn tiền(6) → công việc ĐÃ diễn ra trong khung giờ đó nên thợ vẫn
    ///     bận: không thể vừa làm xong lại nhận thêm đơn khác cùng khung giờ ấy.
    ///
    /// ⚠️ Trước đây điều kiện này bị chép tay ở ba nơi và đã trôi lệch với CSDL (bỏ sót trạng
    /// thái Hoàn thành), khiến khách thấy khung giờ trống rồi mới bị CSDL chặn lúc bấm đặt —
    /// vừa sai thông điệp lỗi vừa hỏng trải nghiệm. Mọi truy vấn liên quan phải dùng lại hàm
    /// này thay vì tự viết điều kiện.
    /// </summary>
    public static class BookingSlotOccupancy
    {
        /// <summary>Thời hạn giữ chỗ của đơn chưa thanh toán; quá hạn thì trả khung giờ về cho người khác.</summary>
        public static readonly TimeSpan HoldTtl = TimeSpan.FromMinutes(15);

        /// <summary>Mốc thời gian mà đơn giữ chỗ tạo trước đó bị coi là đã hết hạn.</summary>
        public static DateTimeOffset FreshHoldSince(DateTimeOffset now) => now - HoldTtl;

        /// <summary>
        /// Vị từ dùng trực tiếp trong <c>Where(...)</c> của EF (dịch được sang SQL).
        /// </summary>
        public static Expression<Func<BookingItem, bool>> Occupying(DateTimeOffset freshHoldSince)
            => b => b.Status != (short)BookingStatus.Cancelled
                    && (b.Status != (short)BookingStatus.Pending || b.CreatedAt > freshHoldSince);
    }
}
