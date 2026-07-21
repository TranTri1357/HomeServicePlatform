using System.Collections.Generic;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Chat.Dtos;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Chat.Queries.GetConversation
{
    public record GetConversationQuery(long BookingId, long UserId)
        : IRequest<ApiResponse<List<MessageDto>>>;
}
