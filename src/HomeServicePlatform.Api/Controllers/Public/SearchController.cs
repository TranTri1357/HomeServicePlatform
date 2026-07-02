using HomeServicePlatform.Application.Modules.Search.Queries;
using HomeServicePlatform.Application.Modules.Search.Queries.GlobalSearch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Public
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SearchController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GlobalSearch([FromQuery] string keyword)
        {
            var result = await _mediator.Send(new GlobalSearchQuery { Keyword = keyword });

            // Sử dụng StatusCode từ ApiResponse để trả về response chuẩn
            return StatusCode(result.StatusCode, result);
        }
    }
}
