using System;
using System.Collections.Generic;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// 🗓️ Lịch làm việc MẶC ĐỊNH cấp cho thợ ngay khi tạo hồ sơ: Thứ 2 → Thứ 7, 08:00–18:00
    /// (giờ Việt Nam). Chủ nhật nghỉ. Thợ vào màn "Lịch làm việc" sửa lại tuỳ ý.
    ///
    /// ❗ VÌ SAO PHẢI SEED, KHÔNG ĐỂ TRỐNG:
    /// Hệ thống chặn đặt lịch ngoài giờ làm việc (xem WorkingHoursGuard). Nếu hồ sơ thợ mới sinh
    /// ra mà KHÔNG có dòng lịch nào thì thợ đó vĩnh viễn không ai đặt được cho tới khi tự vào cấu
    /// hình — một cái bẫy im lặng, thợ không hề biết vì sao mình không có đơn.
    ///
    /// Seed mặc định còn làm dữ liệu có NGHĨA RÕ RÀNG. Sau khi seed, trạng thái "không có dòng
    /// lịch nào" chỉ còn một cách hiểu duy nhất: thợ đã CỐ Ý xoá hết (vd nghỉ dài hạn) — và khi
    /// đó việc không nhận đơn mới là ĐÚNG Ý họ, không phải lỗi hệ thống.
    /// </summary>
    public static class DefaultWorkingSchedule
    {
        /// <summary>Giờ bắt đầu ca mặc định (giờ VN).</summary>
        public static readonly TimeOnly StartTime = new(8, 0);

        /// <summary>Giờ kết thúc ca mặc định (giờ VN).</summary>
        public static readonly TimeOnly EndTime = new(18, 0);

        /// <summary>
        /// Sinh bộ lịch tuần mặc định cho một thợ. DayOfWeek theo <see cref="System.DayOfWeek"/>
        /// (0 = Chủ nhật) để khớp với cách GetTaskerAvailability tra cứu.
        /// </summary>
        public static IEnumerable<TaskerSchedule> For(long taskerId)
        {
            // Thứ 2 (1) → Thứ 7 (6). Bỏ Chủ nhật (0).
            for (short day = 1; day <= 6; day++)
            {
                yield return new TaskerSchedule
                {
                    TaskerId = taskerId,
                    DayOfWeek = day,
                    StartTime = StartTime,
                    EndTime = EndTime
                };
            }
        }
    }
}
