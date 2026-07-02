using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Services.Dtos
{
    public class PopularServiceDto
    {
        public long ServiceId { get; set; }
        public string Name { get; set; } = default!;

        public int TotalBookings { get; set; }

        public decimal StartingPrice { get; set; }
    }
}
