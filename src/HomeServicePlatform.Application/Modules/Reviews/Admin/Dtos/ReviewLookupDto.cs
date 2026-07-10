using System;

namespace HomeServicePlatform.Application.Modules.Reviews.Admin.Dtos
{
    public record ReviewLookupDto(
        long ReviewId,
        string CustomerName,
        string TaskerName,
        string ServiceName,
        short Rating,
        string? Comment,
        DateTimeOffset CreatedAt
    );
}
