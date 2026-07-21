using System.Security.Claims;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetMyWallet;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/wallet")]
    [Authorize(Roles = "Customer")]
    public class CustomerWalletController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerWalletController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WalletDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyWallet()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
                return Unauthorized();

            var result = await _mediator.Send(new GetMyWalletQuery(customerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("topup")]
        [ProducesResponseType(typeof(ApiResponse<decimal>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> TopUp([FromBody] TopUpWalletCommand command)
        {
            if (command == null)
                return BadRequest("Dữ liệu nạp tiền không được để trống.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
                return Unauthorized();

            command.CustomerId = customerId;
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
