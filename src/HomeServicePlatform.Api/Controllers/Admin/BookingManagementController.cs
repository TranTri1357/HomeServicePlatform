using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.UpdateBookingStatus;
using HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "Admin")]
    public class BookingManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BookingManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingLookupDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetPagedBookingsQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateBookingCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu khởi tạo đơn hàng không được để trống.");

            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}/status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateStatus([FromRoute] long id, [FromBody] UpdateBookingStatusCommand command)
        {
            // Bảo mật Id-Spoofing: Ép ID từ Route URL vào Record Command
            var securedCommand = command with { BookingId = id };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}
