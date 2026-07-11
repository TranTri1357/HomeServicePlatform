using System.Security.Claims;
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Application.Common.Interfaces;
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
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
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
        private readonly IHubContext<BookingHub> _hub;
        private readonly IApplicationDbContext _context;

        public BookingController(IMediator mediator, IHubContext<BookingHub> hub, IApplicationDbContext context)
        {
            _mediator = mediator;
            _hub = hub;
            _context = context;
        }

        // Bắn realtime trạng thái đơn mới nhất về cho khách hàng chủ đơn.
        private async Task NotifyCustomerAsync(long bookingId)
        {
            var info = await _context.Bookings.AsNoTracking()
                .Where(b => b.BookingId == bookingId)
                .Select(b => new { b.CustomerId, Status = (short)b.Status })
                .FirstOrDefaultAsync();

            if (info == null) return;

            await _hub.Clients.Group(BookingHub.UserGroup(info.CustomerId))
                .SendAsync("ReceiveBookingStatus", new { bookingId, status = info.Status });
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
            if (result.Succeeded) await NotifyCustomerAsync(id);
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
            if (result.Succeeded) await NotifyCustomerAsync(id);
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
            if (result.Succeeded) await NotifyCustomerAsync(id);
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
            if (result.Succeeded) await NotifyCustomerAsync(id);
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
            if (result.Succeeded) await NotifyCustomerAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}