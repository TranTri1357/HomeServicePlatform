using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/bookings")]
    [Authorize(Roles = "Customer")]
    public class CustomerBookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerBookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResponse<List<MyBookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyBookings()
        {
            // 🛡️ Tự bóc tách ID từ mã Token đã phân mã đăng nhập
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetMyBookingsQuery(customerId));
            return StatusCode(result.StatusCode, result);
        }
    }
}