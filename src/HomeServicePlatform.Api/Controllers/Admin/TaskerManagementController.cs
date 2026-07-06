using HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.ApproveTasker;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.RejectTasker;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Commands.ToggleTaskerStatus;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetAllTaskers;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetTaskerDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [Route("api/admin/taskers")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class TaskerManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public TaskerManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAllTaskers([FromQuery] GetAllTaskersQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTaskerDetail(long id)
        {
            var result = await _mediator.Send(new GetTaskerDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveTasker(long id)
        {
            var result = await _mediator.Send(new ApproveTaskerCommand(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/reject")]
        public async Task<IActionResult> RejectTasker(long id, [FromBody] string? reason)
        {
            var result = await _mediator.Send(new RejectTaskerCommand(id, reason));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleTaskerStatus(long id)
        {
            var result = await _mediator.Send(new ToggleTaskerStatusCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
