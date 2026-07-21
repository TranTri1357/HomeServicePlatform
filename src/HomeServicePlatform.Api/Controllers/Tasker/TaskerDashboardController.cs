using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Commands.SetAvailability;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerDashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker")]
    [Authorize(Roles = "Tasker")]
    public class TaskerDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private bool TryGetTaskerId(out long taskerId)
        {
            taskerId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            return long.TryParse(claim, out taskerId);
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(ApiResponse<TaskerDashboardDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDashboard()
        {
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            var result = await _mediator.Send(new GetTaskerDashboardQuery(taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("availability")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetAvailability([FromBody] SetTaskerAvailabilityCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu không hợp lệ.");
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            command.TaskerId = taskerId;
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
