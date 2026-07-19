namespace HomeServicePlatform.Application.Common.Options
{
    /// <summary>
    /// Các công tắc nghiệp vụ của luồng đặt lịch. Bind từ section "BookingPolicy" trong
    /// appsettings, nên đổi được qua biến môi trường mà KHÔNG cần sửa code hay build lại.
    /// </summary>
    public class BookingPolicyOptions
    {
        public const string SectionName = "BookingPolicy";

        /// <summary>
        /// Bật kiểm tra "giờ hẹn phải nằm trong lịch làm việc của thợ và không rơi vào ngày nghỉ"
        /// khi tạo đơn (xem <c>WorkingHoursGuard</c>). Mặc định BẬT.
        ///
        /// 🔌 CÔNG TẮC THOÁT HIỂM: nếu lúc demo/nghiệm thu phát sinh sự cố dữ liệu lịch làm việc,
        /// tắt bằng biến môi trường <c>BookingPolicy__EnforceWorkingHours=false</c> trên server là
        /// hệ thống quay lại hành vi cũ ngay, không cần deploy lại.
        ///
        /// ⚠️ Tắt đồng nghĩa quay về tin tưởng client: giao diện vẫn chỉ hiện khung giờ hợp lệ,
        /// nhưng gọi thẳng API thì đặt được thợ lúc 3h sáng hoặc đúng ngày thợ xin nghỉ.
        /// Chỉ tắt tạm thời và bật lại ngay sau khi xử lý xong sự cố.
        /// </summary>
        public bool EnforceWorkingHours { get; set; } = true;
    }
}
