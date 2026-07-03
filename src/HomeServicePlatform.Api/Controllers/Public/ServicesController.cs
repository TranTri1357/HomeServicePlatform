using HomeServicePlatform.Application.Modules.Services.Public.Queries.GetPopularServices;
using HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServiceDetail;
using HomeServicePlatform.Application.Modules.Services.Public.Queries.GetServicesExplorer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServicesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("popular")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPopularServices([FromQuery] int limit = 5)
        {
            var query = new GetPopularServicesQuery { Limit = limit };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("explorer")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesExplorer([FromQuery] GetServicesExplorerQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceDetail(long id)
        {
            var query = new GetServiceDetailQuery { ServiceId = id };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
