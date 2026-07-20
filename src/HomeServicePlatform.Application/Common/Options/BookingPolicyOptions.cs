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

        /// <summary>
        /// % giá trị đơn khách phải đặt cọc (phần còn lại trả tiền mặt khi hoàn thành).
        ///
        /// ⚠️ NGUỒN SỰ THẬT DUY NHẤT — dùng ở HAI nơi và bắt buộc phải là cùng một con số:
        ///   1. <c>ProcessCheckout</c>: số tiền thực thu khi khách chọn "Đặt cọc".
        ///   2. <c>RefundPolicy</c>: phần tiền CHỊU RỦI RO khi hủy đơn.
        ///
        /// Nếu hai nơi lệch nhau, khách đặt cọc sẽ bị tính phí hủy trên một khoản khác với
        /// khoản họ đã nộp — sai lệch âm thầm, rất khó phát hiện. Trước đây giá trị này là
        /// hằng số private nằm riêng trong ProcessCheckoutCommandHandler nên dễ lệch.
        /// </summary>
        public int DepositPercent { get; set; } = 30;
    }
}
