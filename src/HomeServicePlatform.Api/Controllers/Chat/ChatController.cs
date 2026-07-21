using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Application.Modules.Chat.Commands.MarkConversationRead;
using HomeServicePlatform.Application.Modules.Chat.Commands.SendMessage;
using HomeServicePlatform.Application.Modules.Chat.Queries.GetConversation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace HomeServicePlatform.Api.Controllers.Chat
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IHubContext<ChatHub> _hub;

        public ChatController(IMediator mediator, IHubContext<ChatHub> hub)
        {
            _mediator = mediator;
            _hub = hub;
        }

        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            return long.TryParse(claim, out userId);
        }

        [HttpGet("{bookingId:long}")]
        public async Task<IActionResult> GetConversation(long bookingId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var result = await _mediator.Send(new GetConversationQuery(bookingId, userId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("{bookingId:long}")]
        public async Task<IActionResult> Send(long bookingId, [FromBody] SendMessageCommand command)
        {
            if (command == null) return BadRequest("Nội dung tin nhắn không được để trống.");
            if (!TryGetUserId(out var userId)) return Unauthorized();

            command.BookingId = bookingId;
            command.SenderId = userId;

            var result = await _mediator.Send(command);

            if (result.Succeeded && result.Data != null)
            {
                await _hub.Clients.Group(ChatHub.GroupName(bookingId))
                    .SendAsync("ReceiveMessage", result.Data);
            }

            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{bookingId:long}/read")]
        public async Task<IActionResult> MarkRead(long bookingId)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var result = await _mediator.Send(new MarkConversationReadCommand(bookingId, userId));
            return StatusCode(result.StatusCode, result);
        }
    }
}
