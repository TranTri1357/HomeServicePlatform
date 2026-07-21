namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerServiceOptions
{
    public record TaskerServiceOptionDto(
        long ServiceId,
        string ServiceName,
        string CategoryName,
        decimal Price,
        int DurationMinutes
    );
}
