using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// 🗓️ Bộ kiểm tra "giờ hẹn có nằm trong lịch làm việc của thợ không" ở tầng ứng dụng.
    ///
    /// ❗ VÌ SAO CẦN: trước đây 3 trong 4 quy tắc quyết định một khung giờ có đặt được hay không
    /// CHỈ tồn tại ở giao diện. Server khi tạo đơn chỉ chặn "trùng giờ với đơn khác" (ràng buộc
    /// CSDL) và "đủ thời gian di chuyển" (TravelBufferGuard) — hoàn toàn không hỏi thợ có làm
    /// việc vào giờ đó không. Gọi thẳng API là đặt được thợ lúc 3h sáng, hoặc đúng ngày thợ đã
    /// xin nghỉ phép. Cùng loại lỗi với việc tin giá do client gửi lên.
    ///
    /// ⚠️ PHẢI KHỚP <c>GetTaskerAvailabilityQueryHandler</c> — nơi sinh danh sách khung giờ cho
    /// giao diện. Nếu hai bên hiểu khác nhau, người dùng sẽ thấy khung giờ trống, bấm đặt, rồi
    /// bị từ chối mà không hiểu vì sao. Ba quy tắc dùng chung:
    ///   1) Thợ phải có <c>TaskerSchedule</c> cho THỨ đó (theo giờ VN)
    ///   2) Giờ hẹn nằm trọn trong [StartTime, EndTime) của ca ngày hôm đó
    ///   3) Không giao với bất kỳ <c>TaskerTimeOff</c> nào
    /// (Quy tắc 4 — không trùng đơn khác + đệm di chuyển — do ràng buộc CSDL và
    ///  <see cref="TravelBufferGuard"/> đảm nhiệm.)
    ///
    /// 🕒 MÚI GIỜ — chỗ dễ sai nhất: <c>TaskerSchedule</c> lưu <c>DayOfWeek</c> + <c>TimeOnly</c>
    /// theo GIỜ VIỆT NAM (UTC+7), còn <c>BookingItem.StartAt/EndAt</c> là <c>DateTimeOffset</c>
    /// mốc UTC. Bắt buộc quy đổi sang giờ VN trước khi so, nếu không sẽ lệch đúng 7 tiếng.
    /// </summary>
    public static class WorkingHoursGuard
    {
        /// <summary>Múi giờ Việt Nam — trùng với hằng số dùng ở GetTaskerAvailability.</summary>
        private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

        /// <summary>
        /// Ném <see cref="BadRequestException"/> nếu khoảng [<paramref name="startAt"/>,
        /// <paramref name="endAt"/>) không nằm trong lịch làm việc của thợ, hoặc rơi vào ngày nghỉ.
        /// </summary>
        public static async Task EnsureWithinWorkingHoursAsync(
            IApplicationDbContext context,
            long taskerId,
            DateTimeOffset startAt,
            DateTimeOffset endAt,
            CancellationToken ct)
        {
            // Quy đổi về giờ VN — đây là hệ quy chiếu mà thợ dùng khi thiết lập lịch.
            var startVn = startAt.ToOffset(VnOffset);
            var endVn = endAt.ToOffset(VnOffset);

            // Đơn vắt qua nửa đêm không thể nằm trọn trong một ca làm việc trong ngày.
            // Chặn sớm với thông báo riêng thay vì để rơi vào nhánh "ngoài giờ" khó hiểu.
            if (startVn.Date != endVn.Date)
            {
                throw new BadRequestException(
                    "Khung giờ đặt không được kéo dài qua ngày hôm sau. Vui lòng tách thành hai lần đặt.");
            }

            short dayOfWeek = (short)startVn.DayOfWeek;

            var schedule = await context.TaskerSchedules.AsNoTracking()
                .Where(s => s.TaskerId == taskerId && s.DayOfWeek == dayOfWeek)
                .Select(s => new { s.StartTime, s.EndTime })
                .FirstOrDefaultAsync(ct);

            // Quy tắc 1 — không có ca nào cho thứ này ⇒ thợ không làm việc ngày đó.
            if (schedule == null)
            {
                throw new BadRequestException(
                    $"Thợ không làm việc vào {VietnameseDayName(startVn.DayOfWeek)}. Vui lòng chọn ngày khác.");
            }

            // Quy tắc 2 — giờ hẹn phải nằm TRỌN trong ca làm việc.
            var itemStart = TimeOnly.FromTimeSpan(startVn.TimeOfDay);
            var itemEnd = TimeOnly.FromTimeSpan(endVn.TimeOfDay);

            if (itemStart < schedule.StartTime || itemEnd > schedule.EndTime)
            {
                throw new BadRequestException(
                    $"Giờ hẹn nằm ngoài giờ làm việc của thợ ({schedule.StartTime:HH\\:mm}–{schedule.EndTime:HH\\:mm} " +
                    $"{VietnameseDayName(startVn.DayOfWeek)}). Vui lòng chọn khung giờ khác.");
            }

            // Quy tắc 3 — không rơi vào kỳ nghỉ phép. So sánh ở mốc UTC vì TimeOff lưu DateTimeOffset.
            bool onTimeOff = await context.TaskerTimeOffs.AsNoTracking()
                .AnyAsync(t => t.TaskerId == taskerId && t.StartAt < endAt && t.EndAt > startAt, ct);

            if (onTimeOff)
            {
                throw new BadRequestException(
                    "Thợ đã báo nghỉ trong khoảng thời gian này. Vui lòng chọn khung giờ khác.");
            }
        }

        private static string VietnameseDayName(DayOfWeek day) => day switch
        {
            DayOfWeek.Monday => "Thứ 2",
            DayOfWeek.Tuesday => "Thứ 3",
            DayOfWeek.Wednesday => "Thứ 4",
            DayOfWeek.Thursday => "Thứ 5",
            DayOfWeek.Friday => "Thứ 6",
            DayOfWeek.Saturday => "Thứ 7",
            _ => "Chủ nhật"
        };
    }
}
