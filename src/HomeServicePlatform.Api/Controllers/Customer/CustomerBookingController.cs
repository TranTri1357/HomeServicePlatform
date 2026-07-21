using System.Security.Claims;
using HomeServicePlatform.Api.Hubs;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CancelBookingByCustomer;
using HomeServicePlatform.Application.Modules.Booking.Commands.CancelEmergencyBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.RebroadcastEmergencyBooking;
using HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview;
using HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IHubContext<BookingHub> _hub;

        public CustomerBookingController(IMediator mediator, IHubContext<BookingHub> hub)
        {
            _mediator = mediator;
            _hub = hub;
        }

        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MyBookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] short[]? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetMyBookingsQuery(
                customerId,
                status is { Length: > 0 } ? status : null,
                search,
                pageIndex,
                pageSize));
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}/cancellation-preview")]
        [ProducesResponseType(typeof(ApiResponse<CancellationPreviewDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCancellationPreview([FromRoute] long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetCancellationPreviewQuery(id, customerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelMyBooking([FromRoute] long id, [FromBody] CancelBookingByCustomerCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            var securedCommand = command with
            {
                BookingId = id,
                CustomerId = customerId
            };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("emergency")]
        [ProducesResponseType(typeof(ApiResponse<CreateEmergencyBookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateEmergency([FromBody] CreateEmergencyBookingCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
                return Unauthorized();

            var securedCommand = command with { CustomerId = customerId };
            var result = await _mediator.Send(securedCommand);

            if (result.Succeeded && result.Data != null)
                await BroadcastOffersAsync(result.Data);

            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("emergency/{id:long}/broadcast")]
        [ProducesResponseType(typeof(ApiResponse<CreateEmergencyBookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RebroadcastEmergency([FromRoute] long id, [FromQuery] double radiusKm)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
                return Unauthorized();

            var result = await _mediator.Send(new RebroadcastEmergencyBookingCommand(customerId, id, radiusKm));

            if (result.Succeeded && result.Data != null)
                await BroadcastOffersAsync(result.Data);

            return StatusCode(result.StatusCode, result);
        }

        private async Task BroadcastOffersAsync(CreateEmergencyBookingResponse d)
        {
            foreach (var t in d.Taskers)
            {
                await _hub.Clients.Group(BookingHub.UserGroup(t.TaskerId))
                    .SendAsync("ReceiveEmergencyRequest", new
                    {
                        bookingId = d.BookingId,
                        serviceName = d.ServiceName,
                        addressLine = d.AddressLine,
                        amount = t.Price,
                        distanceKm = t.DistanceKm,
                        expiresInSeconds = d.ExpiresInSeconds,
                        latitude = d.Latitude,
                        longitude = d.Longitude
                    });
            }
        }

        [HttpPost("emergency/{id:long}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<EmergencyCancelResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelEmergency([FromRoute] long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
                return Unauthorized();

            var result = await _mediator.Send(new CancelEmergencyBookingCommand(id, customerId));

            if (result.Succeeded && result.Data != null)
            {
                foreach (var taskerId in result.Data.NotifyTaskerIds)
                {
                    await _hub.Clients.Group(BookingHub.UserGroup(taskerId))
                        .SendAsync("ReceiveEmergencyCancelled", new { bookingId = id });
                }
            }

            return StatusCode(result.StatusCode, result);
        }
    }
}