using HomeServicePlatform.Domain.Modules.Identity.Entities;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Domain.Modules.Customer.Entities
{
    public class Address
    {
        public long AddressId { get; set; }
        public long UserId { get; set; }
        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }
        public string? WardCode { get; set; }
        public string AddressLine { get; set; } = null!;
        public Point? Geom { get; set; }
        public bool? IsDefault { get; set; } = false;

        public virtual User User { get; set; } = null!;
    }
}
