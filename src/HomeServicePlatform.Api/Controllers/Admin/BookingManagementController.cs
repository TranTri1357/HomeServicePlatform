using System.Security.Claims;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.UpdateBookingStatus;
using HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "Admin")] // Đảm bảo mọi giao dịch tại đây đều yêu cầu quyền Admin
    public class BookingManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BookingManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingLookupDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] GetPagedBookingsQuery query)
        {
        
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateBookingCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu khởi tạo đơn hàng không được để trống.");

            // 🛡️ 2. Xác thực Token cho lệnh tạo đơn
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized(); // 🟢 Đã sửa dùng Cách B
            }

            // Gửi qua MediatR thực thi lưu xuống Database
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}/status")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateStatus([FromRoute] long id, [FromBody] UpdateBookingStatusCommand command)
        {
            // 🛡️ 3. Xác thực Token cho lệnh cập nhật trạng thái
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized(); // 🟢 Đã sửa dùng Cách B
            }

            // 🟢 ĐỒNG BỘ DOANH NGHIỆP: 
            // - Bảo mật Id-Spoofing: Ép ID từ Route URL vào BookingId
            // - Ghi nhận chính xác ai là người duyệt/hủy đơn vào trường ChangedBy từ Token ngầm
            var securedCommand = command with
            {
                BookingId = id,
                ChangedBy = adminId
            };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}