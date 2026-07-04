using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Dtos
{
    public class TopTaskerDto
    {
        public long TaskerId { get; set; }
        public string FullName { get; set; } = default!;
        public string? AvatarUrl { get; set; }
        public decimal RatingAvg { get; set; }
        public int TotalReviews { get; set; }
        public bool IsVerified { get; set; }

        public string? MainSkill { get; set; }
    }
}
