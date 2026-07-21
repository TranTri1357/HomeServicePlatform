using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Chat.Commands.MarkConversationRead
{
    public record MarkConversationReadCommand(long BookingId, long UserId) : IRequest<ApiResponse<bool>>;
}
