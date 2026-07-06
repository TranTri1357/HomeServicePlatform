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

        decimal RatingAvg,
        int TotalReviews,

        short TaskerStatus, // Trạng thái làm nghề (0: Chờ duyệt, 1: Hoạt động, 2: Bị khóa)
        short UserStatus,   // Trạng thái tài khoản gốc (1: Hoạt động, 0: Khóa đăng nhập)

        DateTimeOffset JoinedDate,

        List<string> Skills
    );
}
