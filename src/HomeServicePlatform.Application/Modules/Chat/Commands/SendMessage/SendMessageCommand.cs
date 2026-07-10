using System.Text.Json.Serialization;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Chat.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Chat.Commands.SendMessage
{
    /// <summary>Gửi một tin nhắn vào hội thoại của đơn. BookingId từ route, SenderId từ Token.</summary>
    public class SendMessageCommand : IRequest<ApiResponse<MessageDto>>
    {
        [JsonIgnore]
        public long BookingId { get; set; }

        [JsonIgnore]
        public long SenderId { get; set; }

        public string Content { get; set; } = default!;
    }
}
