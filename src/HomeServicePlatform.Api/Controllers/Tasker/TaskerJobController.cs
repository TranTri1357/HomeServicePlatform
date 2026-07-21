using System.Security.Claims;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobs;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobsPaged;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobStats;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

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

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<TaskerJobDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetJobsByTasker([FromQuery] short? status)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetTaskerJobsQuery(taskerId, status));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("paged")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<TaskerJobGroupDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetJobsPaged(
            [FromQuery] short[]? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
                return Unauthorized();

            var result = await _mediator.Send(new GetTaskerJobsPagedQuery(
                taskerId,
                status is { Length: > 0 } ? status : null,
                search,
                pageIndex,
                pageSize));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("stats")]
        [ProducesResponseType(typeof(ApiResponse<TaskerJobStatsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetJobStats()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
                return Unauthorized();

            var result = await _mediator.Send(new GetTaskerJobStatsQuery(taskerId));
            return StatusCode(result.StatusCode, result);
        }
    }
}