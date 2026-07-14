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

        /// <summary>Số lần thợ hủy trong cửa sổ ngày (dưới đây) đạt ngưỡng này thì tự khóa chờ Admin duyệt.</summary>
        public int TaskerCancelSuspendThreshold { get; set; } = 3;

        /// <summary>Cửa sổ (ngày) để đếm số lần thợ hủy phục vụ tự khóa.</summary>
        public int TaskerCancelWindowDays { get; set; } = 30;
    }
}
