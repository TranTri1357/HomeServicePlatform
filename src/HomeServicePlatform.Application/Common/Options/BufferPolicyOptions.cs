namespace HomeServicePlatform.Application.Common.Options
{
    /// <summary>
    /// Tham số "thời gian đệm di chuyển" (buffer time) giữa hai đơn liên tiếp của cùng một thợ.
    /// Bind từ section "BufferPolicy" trong appsettings nên Admin chỉnh được mà không sửa code.
    ///
    /// Bài toán: chống trùng lịch chỉ so khung giờ là chưa đủ — thợ cần thời gian di chuyển
    /// giữa 2 địa điểm khách khác nhau. Buffer = khoảng cách đường thẳng (PostGIS) / vận tốc
    /// trung bình × hệ số đường vòng, rồi kẹp trong [Min, Max]. Cùng địa chỉ ⇒ buffer = 0.
    /// </summary>
    public class BufferPolicyOptions
    {
        public const string SectionName = "BufferPolicy";

        /// <summary>Vận tốc di chuyển trung bình nội đô (km/h). Xe máy VN ~25.</summary>
        public double AverageSpeedKmh { get; set; } = 25;

        /// <summary>Hệ số bù đường thật so với đường chim bay (đường vòng). ~1.3.</summary>
        public double DetourFactor { get; set; } = 1.3;

        /// <summary>Buffer tối thiểu giữa 2 đơn khác địa chỉ (phút), kể cả khi rất gần nhau.</summary>
        public int MinBufferMinutes { get; set; } = 15;

        /// <summary>Buffer tối đa (phút) — kẹp trần để đơn quá xa không "ăn" hết lịch.</summary>
        public int MaxBufferMinutes { get; set; } = 90;

        /// <summary>
        /// (Thông tin — hiện KHÔNG dùng để tính toán.) Ý tưởng ban đầu định làm "sàn cứng" buffer bằng
        /// ràng buộc EXCLUDE có đệm ở CSDL, nhưng cách đó chặn nhầm đơn nhiều dịch vụ cùng địa điểm nên
        /// đã bỏ. Buffer được thực thi ở tầng ứng dụng (TravelBufferGuard) + advisory lock theo thợ.
        /// Giữ trường này để tham chiếu/cấu hình về sau; MinBufferMinutes chính là sàn thực tế đang dùng.
        /// </summary>
        public int HardFloorMinutes { get; set; } = 20;
    }
}
