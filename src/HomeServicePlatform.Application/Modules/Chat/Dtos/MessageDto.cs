using System;

namespace HomeServicePlatform.Application.Modules.Chat.Dtos
{
    public record MessageDto(
        long MessageId,
        long BookingId,
        long SenderId,
        string SenderName,
        string Content,
        bool IsRead,
        DateTimeOffset CreatedAt
    );
}
