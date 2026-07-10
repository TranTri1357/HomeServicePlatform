using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.MarkNotificationRead;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [Route("api/customer/notifications")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerNotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerNotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Dùng lại query/command thông báo chung (đánh theo UserId, không phụ thuộc vai trò).
        private bool TryGetUserId(out long userId)
        {
            userId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            return long.TryParse(claim, out userId);
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var result = await _mediator.Send(new GetMyNotificationsQuery
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            if (!TryGetUserId(out var userId)) return Unauthorized();

            var result = await _mediator.Send(new MarkNotificationReadCommand
            {
                UserId = userId,
                NotificationId = id
            });
            return StatusCode(result.StatusCode, result);
        }
    }
}
