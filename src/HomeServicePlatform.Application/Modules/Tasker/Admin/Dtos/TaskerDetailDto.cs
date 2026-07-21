using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Dtos
{
    public record TaskerDetailDto(
        long TaskerId,
        string FullName,
        string Email,
        string Phone,

        string? Bio,
        int ExperienceYears,
        bool IsVerified,
        DateTimeOffset? VerifiedAt,

        string? VerificationImageUrl,
        string? RejectionReason,

        decimal RatingAvg,
        int TotalReviews,

        short TaskerStatus,
        short UserStatus,

        DateTimeOffset JoinedDate,

        List<string> Skills
    );
}
