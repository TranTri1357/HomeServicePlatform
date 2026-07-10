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
        public async Task<IActionResult> GetTaskerProfile([FromRoute] long id)
        {
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

        /// <summary>Thợ tự cập nhật thông tin tài khoản của mình.</summary>
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
