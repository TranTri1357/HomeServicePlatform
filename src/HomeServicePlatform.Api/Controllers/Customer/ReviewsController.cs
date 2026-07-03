using HomeServicePlatform.Application.Modules.Reviews.Commands.CreateReview;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [Route("api/bookings/{bookingItemId}/reviews")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class ReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        public async Task<IActionResult> CreateReview(long bookingItemId, [FromBody] CreateReviewCommand command)
        {
            // Lấy ID khách hàng từ JWT Token đang đăng nhập
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện." });

            command.BookingItemId = bookingItemId;
            command.CustomerId = long.Parse(customerIdClaim);

            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}
