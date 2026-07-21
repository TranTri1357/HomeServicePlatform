using System;
using HomeServicePlatform.Application.Common.Options;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class TravelBufferCalculator
    {
        private const double EarthRadiusKm = 6371.0;

        private const double SameLocationKm = 0.05;

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
