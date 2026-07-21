using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Commands.WithdrawWallet;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker/wallet")]
    [Authorize(Roles = "Tasker")]
    public class TaskerWalletController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerWalletController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private bool TryGetTaskerId(out long taskerId)
        {
            taskerId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            return long.TryParse(claim, out taskerId);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<TaskerIncomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetIncome([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            var result = await _mediator.Send(new GetTaskerIncomeQuery(taskerId, page, pageSize));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("withdraw")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Withdraw([FromBody] WithdrawWalletCommand command)
        {
            if (command == null)
                return BadRequest("Dữ liệu rút tiền không được để trống.");

            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            command.TaskerId = taskerId;
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
