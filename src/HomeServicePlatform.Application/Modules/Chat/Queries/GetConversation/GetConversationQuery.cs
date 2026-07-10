using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Chat.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Chat.Queries.GetConversation
{
    /// <summary>Lấy toàn bộ tin nhắn của một đơn. UserId (khách hoặc thợ) từ Token.</summary>
    public record GetConversationQuery(long BookingId, long UserId)
        : IRequest<ApiResponse<List<MessageDto>>>;
}
