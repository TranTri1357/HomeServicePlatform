using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Domain.Modules.Bookings.Entities
{
    public class BookingAddress
    {
        public long BookingAddressId { get; set; }
        public long BookingId { get; set; }
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string AddressLine { get; set; } = null!;
        public Point? Geom { get; set; }

        public virtual Booking Booking { get; set; } = null!;
    }
}
