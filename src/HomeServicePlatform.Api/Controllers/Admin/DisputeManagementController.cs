using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Disputes.Commands.RaiseDispute;
using HomeServicePlatform.Application.Modules.Disputes.Commands.ResolveDispute;
using HomeServicePlatform.Application.Modules.Disputes.Queries.GetPagedDisputes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/disputes")]
    [Authorize(Roles = "Admin,SuperAdmin")] // Đồng nhất quyền với các trang quản trị khác
    public class AdminDisputeController : ControllerBase
    {
        private readonly IMediator _mediator;
        public AdminDisputeController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DisputeLookupDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPaged([FromQuery] GetPagedDisputesQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Raise([FromBody] RaiseDisputeCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}/resolve")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Resolve([FromRoute] long id, [FromBody] ResolveDisputeCommand command)
        {
            // Bảo mật Id-Spoofing: Ép ID từ tuyến đường URL vào Record
            var securedCommand = command with { DisputeId = id };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}
