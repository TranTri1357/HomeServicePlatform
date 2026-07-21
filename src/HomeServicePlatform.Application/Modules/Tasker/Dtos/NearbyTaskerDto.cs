using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Dtos
{
    public class NearbyTaskerDto
    {
        public long TaskerId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public short Status { get; set; }

        public decimal RatingAvg { get; set; }
        public double DistanceKm { get; set; } 
    }
}
