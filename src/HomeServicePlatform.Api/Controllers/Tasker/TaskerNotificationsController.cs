using HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.MarkNotificationRead;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetMyNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [Route("api/tasker/notifications")]
    [ApiController]
    [Authorize(Roles = "Tasker")]
    public class TaskerNotificationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerNotificationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _mediator.Send(new GetMyNotificationsQuery { UserId = userId, PageNumber = pageNumber, PageSize = pageSize });
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkAsRead(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _mediator.Send(new MarkNotificationReadCommand { UserId = userId, NotificationId = id });
            return StatusCode(result.StatusCode, result);
        }
    }
}
