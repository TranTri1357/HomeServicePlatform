using System;
using HomeServicePlatform.Application.Common.Options;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Bộ tính "thời gian đệm di chuyển" (buffer time) giữa hai địa điểm — hàm THUẦN
    /// (không đụng DB), tất định, dễ unit-test. Dùng công thức Haversine để ra khoảng cách
    /// đường chim bay (km), rồi quy ra số phút thợ cần để di chuyển.
    ///
    /// Ý nghĩa nghiệp vụ: chống trùng lịch nếu chỉ so khung giờ là chưa đủ — hai đơn liền kề
    /// ở hai địa điểm xa nhau vẫn khiến thợ trễ giờ. Buffer chính là khoảng trống bắt buộc
    /// giữa giờ kết thúc đơn trước và giờ bắt đầu đơn sau.
    /// </summary>
    public static class TravelBufferCalculator
    {
        private const double EarthRadiusKm = 6371.0;

        // Dưới ngưỡng này coi như CÙNG một địa điểm (vd nhiều dịch vụ tại một nhà) ⇒ buffer = 0.
        private const double SameLocationKm = 0.05; // 50 mét

        /// <summary>
        /// Khoảng cách đường chim bay (km) giữa hai tọa độ theo công thức Haversine.
        /// </summary>
        public static double DistanceKm(double lat1, double lng1, double lat2, double lng2)
        {
            double dLat = ToRad(lat2 - lat1);
            double dLng = ToRad(lng2 - lng1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2))
                       * Math.Sin(dLng / 2) * Math.Sin(dLng / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return EarthRadiusKm * c;
        }

        /// <summary>
        /// Số phút buffer cần cho một quãng đường (km): distance / vận tốc × 60 × hệ số đường vòng,
        /// kẹp trong [Min, Max]. Quãng dưới <see cref="SameLocationKm"/> ⇒ 0 (cùng địa điểm).
        /// </summary>
        public static int BufferMinutes(double distanceKm, BufferPolicyOptions options)
        {
            if (distanceKm < SameLocationKm)
                return 0;

            double speed = options.AverageSpeedKmh > 0 ? options.AverageSpeedKmh : 25;
            double minutes = distanceKm / speed * 60.0 * options.DetourFactor;

            int rounded = (int)Math.Ceiling(minutes);
            if (rounded < options.MinBufferMinutes) rounded = options.MinBufferMinutes;
            if (rounded > options.MaxBufferMinutes) rounded = options.MaxBufferMinutes;
            return rounded;
        }

        /// <summary>
        /// Buffer (phút) giữa hai địa điểm cho trước. Nếu thiếu tọa độ của một trong hai điểm
        /// thì không tính được khoảng cách ⇒ trả về sàn tối thiểu <see cref="BufferPolicyOptions.MinBufferMinutes"/>
        /// để vẫn giữ một khoảng đệm an toàn thay vì bỏ qua.
        /// </summary>
        public static int BufferMinutesBetween(
            double? lat1, double? lng1, double? lat2, double? lng2, BufferPolicyOptions options)
        {
            if (lat1 is null || lng1 is null || lat2 is null || lng2 is null)
                return options.MinBufferMinutes;

            double km = DistanceKm(lat1.Value, lng1.Value, lat2.Value, lng2.Value);
            return BufferMinutes(km, options);
        }

        private static double ToRad(double deg) => deg * Math.PI / 180.0;
    }
}
