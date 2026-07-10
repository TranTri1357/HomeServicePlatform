using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerProfile
{
    public record TaskerProfileDto(
        long TaskerProfileId,
        string FullName,
        string Phone,
        string Email,
        int ExperienceYears,
        decimal RatingAvg,
        int TotalReviews,
        int CompletedJobsCount, // Số lượng việc đã thực hiện dựa trên bookingitem
        short Status,
        string? Bio
    );
}
