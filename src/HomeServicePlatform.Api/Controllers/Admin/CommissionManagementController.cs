using HomeServicePlatform.Application.Modules.Commissions.Commands.CreateCommission;
using HomeServicePlatform.Application.Modules.Commissions.Commands.TerminateCommission;
using HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission;
using HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions;
using HomeServicePlatform.Application.Modules.Commissions.Queries.GetCommissionDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/commissions")]
    [Authorize(Roles = "Tasker")] // Đảm bảo chỉ có Quản trị tài chính mới được vào cấu hình % sàn
    public class CommissionManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CommissionManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCommissionsQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetDetail([FromRoute] long id)
        {
            var result = await _mediator.Send(new GetCommissionDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommissionCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateCommissionCommand command)
        { 
            var securedCommand = command with { CommissionId = id };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Terminate([FromRoute] long id)
        {
            var result = await _mediator.Send(new TerminateCommissionCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}
