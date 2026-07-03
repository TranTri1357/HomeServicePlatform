using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartMoving;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartWorking;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker/bookings")]
    // 🟢 Đăng ký gộp bộ bẫy lỗi 400/500 của Middleware tại đây để các hàm bên dưới không phải viết lại
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{id}/accept")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Accept(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new AcceptBookingCommand(id, body.TaskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/start-moving")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartMoving(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new StartMovingCommand(id, body.TaskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/start-working")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartWorking(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new StartWorkingCommand(id, body.TaskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id}/complete-work")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteWork(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new CompleteWorkCommand(id, body.TaskerId));
            return StatusCode(result.StatusCode, result);
        }
        [HttpPut("{id}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelBooking([FromBody] CancelBookingCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }

    public record TaskerActionBody(long TaskerId);
}
