using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Dtos
{
    public class ServiceExplorerDto
    {
        public long ServiceId { get; set; }
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public int DurationMinutes { get; set; }

        public int TotalBookings { get; set; }

        public decimal StartingPrice { get; set; }

        /// <summary>Điểm đánh giá TB của các thợ (đã có review) cung cấp dịch vụ này; 0 = chưa có.</summary>
        public decimal AvgRating { get; set; }

        public string? ImageUrl { get; set; }
    }
}
