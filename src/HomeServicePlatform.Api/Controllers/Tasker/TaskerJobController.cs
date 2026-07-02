using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker/tasker-jobs")]
    [Authorize(Roles = "Tasker")]
    public class TaskerJobController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerJobController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("{taskerId:long}")]
        [ProducesResponseType(typeof(ApiResponse<List<TaskerJobDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetJobsByTasker([FromRoute] long taskerId, [FromQuery] short? status)
        {
            var result = await _mediator.Send(new GetTaskerJobsQuery(taskerId, status));
            return StatusCode(result.StatusCode, result);
        }
    }
}
