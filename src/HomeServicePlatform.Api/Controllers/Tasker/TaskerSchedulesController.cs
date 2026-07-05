using HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.CreateTimeOff;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.DeleteTimeOff;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Commands.UpdateWeeklySchedule;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetDailySchedule;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [Route("api/tasker/schedule")]
    [ApiController]
    [Authorize(Roles = "Tasker")]
    public class TaskerSchedulesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerSchedulesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailySchedule([FromQuery] DateOnly date)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var query = new GetDailyScheduleQuery { UserId = userId, Date = date };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("weekly")]
        public async Task<IActionResult> UpdateWeeklySchedule([FromBody] List<DailyScheduleInput> schedules)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var command = new UpdateWeeklyScheduleCommand { UserId = userId, Schedules = schedules };
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("time-off")]
        public async Task<IActionResult> RequestTimeOff([FromBody] CreateTimeOffCommand command)
        {
            command.UserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("time-off/{id}")]
        public async Task<IActionResult> CancelTimeOff(long id)
        {
            var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var command = new DeleteTimeOffCommand { UserId = userId, TimeOffId = id };
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
