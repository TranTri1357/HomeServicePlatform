namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerServiceOptions
{
    /// <summary>Một dịch vụ + giá mà thợ cung cấp — dùng cho khách chọn khi đặt lịch.</summary>
    public record TaskerServiceOptionDto(
        long ServiceId,
        string ServiceName,
        string CategoryName,
        decimal Price,
        int DurationMinutes
    );
}
