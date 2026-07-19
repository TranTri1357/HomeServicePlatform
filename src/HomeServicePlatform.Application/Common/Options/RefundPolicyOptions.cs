namespace HomeServicePlatform.Application.Common.Options
{
    /// <summary>
    /// Tham số chính sách hủy/hoàn tiền. Bind từ section "RefundPolicy" trong appsettings
    /// nên Admin chỉnh được mà không phải sửa code (đúng yêu cầu "thiết lập điều kiện hủy hoàn cọc").
    /// </summary>
    public class RefundPolicyOptions
    {
        public const string SectionName = "RefundPolicy";

        /// <summary>Khách hủy khi thợ ĐÃ nhận (Accepted) mà còn > số giờ này tới giờ hẹn thì hoàn 100%.</summary>
        public double FreeCancelHours { get; set; } = 2;

        /// <summary>% hoàn cho khách khi hủy lúc Accepted nhưng còn ≤ FreeCancelHours tới giờ hẹn.</summary>
        public int LateAcceptedRefundPercent { get; set; } = 50;

        /// <summary>% hoàn cho khách khi hủy lúc thợ đang trên đường (OnTheWay).</summary>
        public int OnTheWayRefundPercent { get; set; } = 0;

        /// <summary>
        /// Tổng số lần thợ CHỦ ĐỘNG BỎ đơn đã nhận (TaskerProfile.CancelCount) đạt ngưỡng này
        /// thì tự KHÓA tài khoản (User.Status = 0), chờ Admin mở lại. Từ chối đơn khẩn không tính.
        /// </summary>
        public int TaskerCancelSuspendThreshold { get; set; } = 3;

        /// <summary>
        /// Đơn ĐÃ THU TIỀN mà thợ không bấm xác nhận trong ngần này phút kể từ lúc tạo thì hệ thống
        /// tự hủy và hoàn 100% cho khách.
        ///
        /// Cố tình KHÔNG dùng chung mốc 15 phút của BookingSlotOccupancy: mốc đó là TTL giữ chỗ cho
        /// đơn CHƯA trả tiền (nhả slot cho người khác đặt), còn đây là hạn phản hồi của thợ trên một
        /// đơn khách đã bỏ tiền — đáng được chờ lâu hơn.
        /// </summary>
        public int TaskerConfirmDeadlineMinutes { get; set; } = 30;
    }
}
