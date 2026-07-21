using System.Security.Claims;
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CancelBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork;
using HomeServicePlatform.Application.Modules.Booking.Commands.DeclineEmergencyBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.DeclinePendingBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartMoving;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartWorking;
using HomeServicePlatform.Application.Modules.Booking.Emergency;
using MediatR;
using System.Linq;
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
    [Authorize(Roles = "Tasker")]
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new AcceptBookingCommand(id, taskerId));
            if (result.Succeeded)
            {
                await NotifyCustomerAsync(id);
                await NotifyOtherEmergencyTaskersAsync(id, winnerTaskerId: taskerId);
            }
            return StatusCode(result.StatusCode, result);
        }

        private async Task NotifyOtherEmergencyTaskersAsync(long bookingId, long winnerTaskerId)
        {
            var info = await _context.Bookings.AsNoTracking()
                .Where(b => b.BookingId == bookingId && b.IsEmergency)
                .Select(b => new
                {
                    ServiceId = b.BookingItems.Select(i => i.ServiceId).FirstOrDefault(),
                    Lat = b.BookingAddress != null && b.BookingAddress.Geom != null ? (double?)b.BookingAddress.Geom.Y : null,
                    Lng = b.BookingAddress != null && b.BookingAddress.Geom != null ? (double?)b.BookingAddress.Geom.X : null
                })
                .FirstOrDefaultAsync();

            if (info == null || info.Lat == null || info.Lng == null) return;

            var taskers = await EmergencyTaskerFinder.FindEligibleAsync(
                _context, info.ServiceId, info.Lat.Value, info.Lng.Value, radiusKm: 15.0, ct: default);

            foreach (var t in taskers)
            {
                if (t.TaskerId == winnerTaskerId) continue;
                await _hub.Clients.Group(BookingHub.UserGroup(t.TaskerId))
                    .SendAsync("ReceiveEmergencyCancelled", new { bookingId });
            }
        }

        [HttpPut("{id:long}/start-moving")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartMoving([FromRoute] long id)
        {
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
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new CompleteWorkCommand(id, taskerId));
            if (result.Succeeded) await NotifyCustomerAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("emergency/{id:long}/decline")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeclineEmergency([FromRoute] long id, [FromQuery] bool timedOut = false)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
                return Unauthorized();

            var result = await _mediator.Send(new DeclineEmergencyBookingCommand(id, taskerId, timedOut));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/decline")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeclineBooking(
            [FromRoute] long id, [FromBody] DeclinePendingBookingCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
                return Unauthorized();

            var securedCommand = command with { BookingId = id, TaskerId = taskerId };

            var result = await _mediator.Send(securedCommand);
            if (result.Succeeded) await NotifyCustomerAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CancelBooking([FromRoute] long id, [FromBody] CancelBookingCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var securedCommand = command with
            {
                BookingId = id,
                TaskerId = taskerId
            };

            var result = await _mediator.Send(securedCommand);
            if (result.Succeeded) await NotifyCustomerAsync(id);
            return StatusCode(result.StatusCode, result);
        }
    }
}