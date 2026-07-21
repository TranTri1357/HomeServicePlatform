using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Dtos
{
    public class ServiceDetailDto
    {
        public long ServiceId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }

        public int TotalBookings { get; set; }

        public decimal StartingPrice { get; set; }
        public string? ImageUrl { get; set; }

        public List<ServiceTaskerSuggestionDto> SuggestedTaskers { get; set; } = new();
    }

    public class ServiceTaskerSuggestionDto
    {
        public long TaskerId { get; set; }
        public string FullName { get; set; } = default!;
        public string? AvatarUrl { get; set; }
        public int ExperienceYears { get; set; }
        public decimal RatingAvg { get; set; }
        public decimal CurrentPrice { get; set; }

        public string? ProvinceCode { get; set; }
        public string? DistrictCode { get; set; }

        public double? DistanceKm { get; set; }
    }
}
