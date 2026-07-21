using System.Text.Json;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Enum;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class NotificationBuilder
    {
        public static Notification Build(long userId, NotificationType type, string title, string body)
        {
            var payload = JsonSerializer.Serialize(new { title, body });
            return new Notification
            {
                UserId = userId,
                Type = (short)type,
                Payload = payload,
                Status = 0
            };
        }
    }
}
