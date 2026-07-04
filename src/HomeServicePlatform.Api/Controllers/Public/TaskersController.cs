using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTaskerDetail;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetTopTaskers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskersController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTaskerDetail(long id)
        {
            var query = new GetTaskerDetailQuery { TaskerId = id };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("top")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTopTaskers([FromQuery] int limit = 5)
        {
            var query = new GetTopTaskersQuery { Limit = limit };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
