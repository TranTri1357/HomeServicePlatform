using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Dtos
{
    public class AddressDto
    {
        public long AddressId { get; set; }
        public string ProvinceCode { get; set; } = default!;
        public string DistrictCode { get; set; } = default!;
        public string WardCode { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public bool IsDefault { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
