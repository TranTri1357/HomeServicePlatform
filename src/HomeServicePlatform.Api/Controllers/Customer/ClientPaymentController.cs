using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Payments.Commands.ConfirmMockPayment;
using HomeServicePlatform.Application.Modules.Payments.Commands.ProcessCheckout;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/payments")]
    [Authorize(Roles = "Customer")]
    public class ClientPaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ClientPaymentController(IMediator mediator) => _mediator = mediator;

        
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Checkout([FromBody] ProcessCheckoutCommand command)
        {
            if (command == null)
            {
                return BadRequest("Dữ liệu cấu trúc thanh toán gửi lên không được phép để trống.");
            }

             
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }
             
            var securedCommand = command with { CustomerId = customerId };

            var result = await _mediator.Send(securedCommand);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("mock/confirm")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmMockPayment([FromBody] ConfirmMockPaymentCommand command)
        {
            if (command == null)
                return BadRequest("Dữ liệu xác nhận thanh toán không được để trống.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            var securedCommand = command with { CustomerId = customerId };

            var result = await _mediator.Send(securedCommand);

            return StatusCode(result.StatusCode, result);
        }
    }
}