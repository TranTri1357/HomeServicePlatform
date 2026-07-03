using HomeServicePlatform.Application.Modules.Disputes.Commands.CreateDispute;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [Route("api/bookings/{bookingId}/disputes")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class DisputesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public DisputesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDispute(long bookingId, [FromBody] CreateDisputeCommand command)
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện." });

            command.BookingId = bookingId;
            command.CustomerId = long.Parse(customerIdClaim);

            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
