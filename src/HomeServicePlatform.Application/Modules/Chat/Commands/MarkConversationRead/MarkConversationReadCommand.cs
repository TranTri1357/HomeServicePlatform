using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Chat.Commands.MarkConversationRead
{
    /// <summary>Đánh dấu đã đọc mọi tin nhắn của phía bên kia trong đơn. UserId từ Token.</summary>
    public record MarkConversationReadCommand(long BookingId, long UserId) : IRequest<ApiResponse<bool>>;
}
