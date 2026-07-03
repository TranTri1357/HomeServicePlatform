using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Dtos
{
    public class TaskerDetailDto
    {
        public long TaskerId { get; set; }
        public string FullName { get; set; } = default!;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsVerified { get; set; }
        public int ExperienceYears { get; set; }

        public decimal RatingAvg { get; set; }
        public int TotalReviews { get; set; }
        public int TotalJobs { get; set; } // Tổng số công việc đã hoàn thành

        // Kỹ năng chuyên môn (Tên các dịch vụ thợ làm)
        public List<string> Skills { get; set; } = new();

        // Chứng chỉ (Placeholder chờ Entity sau này)
        public List<string> Certificates { get; set; } = new();

        public ReviewSummaryDto ReviewSummary { get; set; } = new();
        public List<TaskerReviewDto> RecentReviews { get; set; } = new();
    }
    public class ReviewSummaryDto
    {
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }
    }

    public class TaskerReviewDto
    {
        public long ReviewId { get; set; }
        public string CustomerName { get; set; } = default!;
        public string? CustomerAvatarUrl { get; set; }
        public short Rating { get; set; }
        public string? Comment { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
