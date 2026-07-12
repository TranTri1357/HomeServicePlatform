using HomeServicePlatform.Application.Modules.Customer.Commands.CreateAddress;
using HomeServicePlatform.Application.Modules.Customer.Commands.DeleteAddress;
using HomeServicePlatform.Application.Modules.Customer.Commands.SetDefaultAddress;
using HomeServicePlatform.Application.Modules.Customer.Commands.UpdateAddress;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetCustomerAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    /// <summary>
    /// Địa chỉ hoạt động của thợ (Phương án B: dùng chung bảng Address, khóa theo
    /// UserId — mà UserId của thợ chính là TaskerProfileId). Tái sử dụng nguyên các
    /// command/query địa chỉ của khách, chỉ ép UserId từ Token của thợ.
    /// </summary>
    [Route("api/tasker/addresses")]
    [ApiController]
    [Authorize(Roles = "Tasker")]
    public class TaskerAddressesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TaskerAddressesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Bóc tách TaskerId (== UserId) từ Token; trả về false nếu không hợp lệ.
        private bool TryGetTaskerId(out long taskerId)
        {
            taskerId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            return long.TryParse(claim, out taskerId);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            if (!TryGetTaskerId(out var taskerId))
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện." });

            var result = await _mediator.Send(new GetCustomerAddressesQuery { CustomerId = taskerId });
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu địa chỉ không được để trống.");
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            command.CustomerId = taskerId; // 🔒 Ép UserId từ Token
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateAddressCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu địa chỉ không được để trống.");
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            command.AddressId = id;
            command.CustomerId = taskerId; // 🔒 Ép UserId từ Token
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            var result = await _mediator.Send(new DeleteAddressCommand(id, taskerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/default")]
        public async Task<IActionResult> SetDefault([FromRoute] long id)
        {
            if (!TryGetTaskerId(out var taskerId)) return Unauthorized();

            var result = await _mediator.Send(new SetDefaultAddressCommand(id, taskerId));
            return StatusCode(result.StatusCode, result);
        }
    }
}
