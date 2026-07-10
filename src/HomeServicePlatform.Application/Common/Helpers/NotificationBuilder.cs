using System.Text.Json;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using HomeServicePlatform.Domain.Modules.Operations.Enum;

namespace HomeServicePlatform.Application.Common.Helpers
{
    /// <summary>
    /// Tạo thực thể Notification với payload JSON { title, body } — khớp cách
    /// Frontend đọc (parsePayload). CreatedAt được DbContext tự gán khi lưu.
    /// </summary>
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
                Status = 0 // 0 = chưa đọc
            };
        }
    }
}
