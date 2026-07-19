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

        // Danh sách đơn của khách: LỌC theo trạng thái (?status=0&status=1...), TÌM (?search=)
        // và PHÂN TRANG (?pageIndex=&pageSize=) — không tải toàn bộ như trước.
        [HttpGet("my-orders")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MyBookingDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyBookings(
            [FromQuery] short[]? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageIndex = 1,
            [FromQuery] int pageSize = 10)
        {
            // 🛡️ Tự bóc tách ID từ mã Token đã phân mã đăng nhập
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

        // 👁️ Xem trước số tiền được hoàn / phí hủy TRƯỚC khi khách bấm hủy (không ghi DB).
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
            // 🛡️ Tự bóc tách ID của Khách hàng từ mã Token đã phân mã đăng nhập
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            // 🔒 CHỐNG ID-SPOOFING: Đè chặt BookingId từ Route URL và CustomerId từ Token vào Command
            var securedCommand = command with
            {
                BookingId = id,
                CustomerId = customerId
            };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        // 🚨 Khách gọi thợ khẩn cấp (BROADCAST): tạo 1 đơn treo mở rồi bắn SignalR tới TẤT CẢ thợ rảnh
        //    trong bán kính đầu (5km). Ai bấm nhận trước thì được đơn.
        [HttpPost("emergency")]
        [ProducesResponseType(typeof(ApiResponse<CreateEmergencyBookingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateEmergency([FromBody] CreateEmergencyBookingCommand command)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
                return Unauthorized();

            // 🔒 Đè CustomerId từ Token, chống spoofing.
            var securedCommand = command with { CustomerId = customerId };
            var result = await _mediator.Send(securedCommand);

            if (result.Succeeded && result.Data != null)
                await BroadcastOffersAsync(result.Data);

            return StatusCode(result.StatusCode, result);
        }

        // 🚨 Nới bán kính quét (5km → 10km → 15km) cho đơn khẩn cấp chưa ai nhận, rồi bắn SignalR
        //    tới các thợ trong vòng mới. Frontend gọi khi hết một vòng 30s mà chưa có thợ nhận.
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

        // Bắn "ReceiveEmergencyRequest" tới từng thợ đủ điều kiện — mỗi thợ nhận GIÁ RIÊNG của mình.
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

        // 🚨 Khách hủy đơn khẩn (hết 30s / chọn thợ khác) → báo thợ đóng modal.
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
                // Đơn khẩn là BROADCAST nên TaskerId = 0 (chưa gán ai). Trước đây chỗ này gửi tới
                // UserGroup(0) — không ai nhận, khiến modal bên thợ vẫn kêu chuông hết 30s dù khách
                // đã hủy. Giờ bắn tới đúng danh sách thợ đã được broadcast.
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