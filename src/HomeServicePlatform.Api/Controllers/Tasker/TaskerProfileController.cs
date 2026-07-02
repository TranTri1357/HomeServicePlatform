using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    }
}
