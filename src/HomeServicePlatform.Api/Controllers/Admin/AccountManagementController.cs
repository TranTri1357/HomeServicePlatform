using HomeServicePlatform.Application.Modules.Identity.Admin.Commands.ToggleUserStatus;
using HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetAllUsers;
using HomeServicePlatform.Application.Modules.Identity.Admin.Queries.GetUserDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AccountManagementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AccountManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetail(long id)
        {
            var result = await _mediator.Send(new GetUserDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(long id)
        {
            var result = await _mediator.Send(new ToggleUserStatusCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
