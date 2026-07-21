using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile;
using HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerProfile;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker/profile")]
    [Authorize(Roles = "Tasker")]
    public class AdminTaskerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminTaskerController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("{id:long}/profile")]
        [ProducesResponseType(typeof(ApiResponse<TaskerProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetTaskerProfile([FromRoute] long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long tokenUserId))
                return Unauthorized();

            if (id != tokenUserId)
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Failure("Bạn chỉ có thể xem hồ sơ của chính mình."));

            var result = await _mediator.Send(new GetTaskerProfileQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProfile([FromBody] CreateTaskerProfileCommand command)
        {
            command.UserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateTaskerProfileCommand command)
        {
            command.UserId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
