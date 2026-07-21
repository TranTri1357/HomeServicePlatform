using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Commands.AddTaskerService;
using HomeServicePlatform.Application.Modules.Tasker.Commands.RemoveTaskerService;
using HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerServicePrice;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker-services")]
    [Authorize(Roles = "Tasker")]
    public class TaskerServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerServiceController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TaskerServiceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetServicesByTasker()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetTaskerServicesQuery(taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddService([FromBody] AddTaskerServiceCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu đăng ký dịch vụ không được để trống.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var securedCommand = command with { TaskerId = taskerId };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("update-price")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePrice([FromBody] UpdateTaskerServicePriceCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu cập nhật giá không được để trống.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var securedCommand = command with { TaskerId = taskerId };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("services/{serviceId:long}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveService([FromRoute] long serviceId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new RemoveTaskerServiceCommand(taskerId, serviceId));
            return StatusCode(result.StatusCode, result);
        }
    }
}