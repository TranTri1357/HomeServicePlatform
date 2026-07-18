using HomeServicePlatform.Application.Modules.Categories.Queries.GetActiveCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HomeServicePlatform.Api.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("active")]
        [AllowAnonymous]
        [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "*" })]
        public async Task<IActionResult> GetActiveCategories([FromQuery] int? limit)
        {
            var query = new GetActiveCategoriesQuery { Limit = limit };
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
