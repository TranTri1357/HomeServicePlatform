using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartMoving;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartWorking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker/bookings")]
    [Authorize(Roles = "Tasker")] // 🛡️ BẢO MẬT: Chỉ cho phép tài khoản Thợ (Tasker) thao tác quy trình
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPut("{id:long}/accept")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Accept([FromRoute] long id)
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new AcceptBookingCommand(id, taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/start-moving")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartMoving([FromRoute] long id)
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new StartMovingCommand(id, taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/start-working")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartWorking([FromRoute] long id)
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new StartWorkingCommand(id, taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/complete-work")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteWork([FromRoute] long id)
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new CompleteWorkCommand(id, taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelBooking([FromRoute] long id, [FromBody] CancelBookingCommand command)
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            // 🔒 CHỐNG ID-SPOOFING: Đè chặt ID an toàn từ Route URL và Token người thực hiện vào Record Command
            var securedCommand = command with
            {
                BookingId = id,
                //CancelledBy = taskerId
            };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}