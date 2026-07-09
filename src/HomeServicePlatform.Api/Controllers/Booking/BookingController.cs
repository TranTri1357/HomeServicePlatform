using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Booking
{
    [ApiController]
    [Route("api/customer/bookings")]
    [Authorize(Roles = "Customer")] // 🛡️ Chỉ cho phép người dùng có quyền Khách hàng truy cập
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateBookingCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu khởi tạo đơn hàng không được để trống.");

            // 🛡️ Xác thực Token ngầm và tự động bóc tách ID của Khách hàng
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized(); // 🟢 Áp dụng Cách B tinh gọn
            }

            // 🔒 BẢO MẬT: Ép buộc ghi đè CustomerId lấy trực tiếp từ Token ngầm vào Command, 
            // chặn đứng hoàn toàn việc gửi giả mạo ID từ phía Frontend Client.
            var securedCommand = command with { CustomerId = customerId };

            // 1. MediatR tự kích hoạt Pipeline Validation chặn dữ liệu rác đầu vào.
            var result = await _mediator.Send(securedCommand);

            // 2. Trả về HTTP 201 Created kèm bọc khối dữ liệu đồng nhất toàn hệ thống.
            return StatusCode(result.StatusCode, result);
        }
    }
}