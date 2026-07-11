using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
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

        /// <summary>Ví/thu nhập của thợ đang đăng nhập: số dư + lịch sử thực nhận (đã trừ hoa hồng).</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<TaskerIncomeDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetIncome([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            var result = await _mediator.Send(new GetTaskerIncomeQuery(taskerId, page, pageSize));
            return StatusCode(result.StatusCode, result);
        }
    }
}
