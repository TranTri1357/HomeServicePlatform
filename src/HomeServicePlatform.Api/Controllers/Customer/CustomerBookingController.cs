using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/bookings")]
    public class CustomerBookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerBookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResponse<List<MyBookingDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyBookings([FromQuery] long customerId)
        {
            var result = await _mediator.Send(new GetMyBookingsQuery(customerId));
            return StatusCode(result.StatusCode, result);
        }
    }
}
