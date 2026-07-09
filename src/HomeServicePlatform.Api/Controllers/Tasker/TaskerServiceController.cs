using System.Security.Claims;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Commands.AddTaskerService;
using HomeServicePlatform.Application.Modules.Tasker.Commands.RemoveTaskerService;
using HomeServicePlatform.Application.Modules.Tasker.Commands.UpdateTaskerServicePrice;
using HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerServices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker-services")]
    [Authorize(Roles = "Tasker")] // 🛡️ BẢO MẬT: Chỉ cho phép tài khoản Thợ thao tác danh mục dịch vụ cá nhân
    public class TaskerServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerServiceController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// API Lấy danh sách toàn bộ các dịch vụ mà Thợ đang đăng nhập đã đăng ký làm việc
        /// </summary>
        [HttpGet] // 🟢 Gỡ bỏ hoàn toàn biến lộ thô "{taskerId:long}" trên URL
        [ProducesResponseType(typeof(ApiResponse<List<TaskerServiceDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetServicesByTasker()
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized(); // 🟢 Cách B tinh gọn
            }

            var result = await _mediator.Send(new GetTaskerServicesQuery(taskerId));
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// API Đăng ký thêm dịch vụ làm việc mới cho Thợ
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddService([FromBody] AddTaskerServiceCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu đăng ký dịch vụ không được để trống.");

            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            // 🔒 CHỐNG ID-SPOOFING: Ghi đè cứng TaskerId từ Token vào Body Command
            var securedCommand = command with { TaskerId = taskerId };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// API Cập nhật giá sàn dịch vụ do Thợ tự cấu hình
        /// </summary>
        [HttpPut("update-price")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePrice([FromBody] UpdateTaskerServicePriceCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu cập nhật giá không được để trống.");

            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            // 🔒 CHỐNG ID-SPOOFING: Ghi đè cứng TaskerId từ Token vào Body Command
            var securedCommand = command with { TaskerId = taskerId };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// API Xóa dịch vụ đã đăng ký ra khỏi hồ sơ làm việc của Thợ
        /// </summary>
        [HttpDelete("services/{serviceId:long}")] // 🟢 Rút gọn tuyến đường, loại bỏ hoàn toàn tham số taskerId trên URL
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveService([FromRoute] long serviceId)
        {
            // 🛡️ Tự động bóc tách ID của Thợ từ chuỗi mã Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long taskerId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new RemoveTaskerServiceCommand(taskerId, serviceId));
            return StatusCode(result.StatusCode, result);
        }
    }
}