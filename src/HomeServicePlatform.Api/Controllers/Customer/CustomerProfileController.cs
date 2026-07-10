using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Customer.Commands.UpdateCustomerProfile;
using HomeServicePlatform.Application.Modules.Customer.Queries.GetCustomerProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer-profile")]
    [Authorize(Roles = "Customer")] // 🛡️ BẢO MẬT: Chỉ cho phép tài khoản Khách hàng truy cập hồ sơ cá nhân
    public class CustomerProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// API Lấy thông tin chi tiết hồ sơ cá nhân của Khách hàng đang đăng nhập
        /// </summary>
        [HttpGet] // 🟢 Loại bỏ hoàn toàn bẫy lộ tham số "{customerId:long}" trên URL
        [ProducesResponseType(typeof(ApiResponse<CustomerProfileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetProfile()
        {
            // 🛡️ Tự động bóc tách ID khách hàng từ chuỗi mã Token đã phân mã đăng nhập ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized(); // 🟢 Áp dụng Cách B gọn gàng, an toàn tuyệt đối
            }

            // Gửi customerId lấy từ Token đi để truy vấn thông tin Profile chính xác
            var result = await _mediator.Send(new GetCustomerProfileQuery(customerId));
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// API cập nhật hồ sơ (họ tên + số điện thoại) của khách hàng đang đăng nhập.
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateCustomerProfileCommand command)
        {
            if (command == null)
                return BadRequest("Dữ liệu cập nhật hồ sơ không được để trống.");

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long customerId))
            {
                return Unauthorized();
            }

            command.CustomerId = customerId; // 🔒 Ép từ Token, chặn giả mạo
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }
    }
}