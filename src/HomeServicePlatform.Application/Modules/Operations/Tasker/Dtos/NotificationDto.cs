using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Dtos
{
    public class NotificationDto
    {
        public long NotificationId { get; set; }
        public short Type { get; set; }
        public string? Payload { get; set; } // Chứa chuỗi JSON (Title, Body, Data liên kết)
        public bool IsRead { get; set; } // Dịch từ Status sang bool cho Frontend dễ dùng
        public DateTimeOffset CreatedAt { get; set; }
    }
}
