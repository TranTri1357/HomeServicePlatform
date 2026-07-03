using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Commands.AddTaskerService;
using HomeServicePlatform.Application.Modules.Tasker.Commands.RemoveTaskerService;
using HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerServicePrice;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker-services")]
    [Authorize(Roles = "Tasker")]
    public class TaskerServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerServiceController(IMediator mediator) => _mediator = mediator;
        [HttpGet("{taskerId:long}")]
        [ProducesResponseType(typeof(ApiResponse<List<TaskerServiceDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetServicesByTasker([FromRoute] long taskerId)
        {
            var result = await _mediator.Send(new GetTaskerServicesQuery(taskerId));
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> AddService([FromBody] AddTaskerServiceCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("update-price")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdatePrice([FromBody] UpdateTaskerServicePriceCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
        [HttpDelete("{taskerId:long}/services/{serviceId:long}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> RemoveService([FromRoute] long taskerId, [FromRoute] long serviceId)
        {
            var result = await _mediator.Send(new RemoveTaskerServiceCommand(taskerId, serviceId));
            return StatusCode(result.StatusCode, result);
        }
    }
}
