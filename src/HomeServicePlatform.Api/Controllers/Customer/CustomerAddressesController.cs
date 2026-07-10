using HomeServicePlatform.Application.Modules.Customer.Commands.CreateAddress;
using HomeServicePlatform.Application.Modules.Customer.Commands.DeleteAddress;
using HomeServicePlatform.Application.Modules.Customer.Commands.SetDefaultAddress;
using HomeServicePlatform.Application.Modules.Customer.Commands.UpdateAddress;
using HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetCustomerAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [Route("api/customer/addresses")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CustomerAddressesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerAddressesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Bóc tách CustomerId từ Token; trả về false nếu không hợp lệ.
        private bool TryGetCustomerId(out long customerId)
        {
            customerId = 0;
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("uid")?.Value;
            return long.TryParse(claim, out customerId);
        }

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            if (!TryGetCustomerId(out var customerId))
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện." });

            var result = await _mediator.Send(new GetCustomerAddressesQuery { CustomerId = customerId });
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAddressCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu địa chỉ không được để trống.");
            if (!TryGetCustomerId(out var customerId)) return Unauthorized();

            command.CustomerId = customerId; // 🔒 Ép từ Token
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateAddressCommand command)
        {
            if (command == null) return BadRequest("Dữ liệu địa chỉ không được để trống.");
            if (!TryGetCustomerId(out var customerId)) return Unauthorized();

            command.AddressId = id;
            command.CustomerId = customerId; // 🔒 Ép từ Token
            var result = await _mediator.Send(command);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            if (!TryGetCustomerId(out var customerId)) return Unauthorized();

            var result = await _mediator.Send(new DeleteAddressCommand(id, customerId));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPut("{id:long}/default")]
        public async Task<IActionResult> SetDefault([FromRoute] long id)
        {
            if (!TryGetCustomerId(out var customerId)) return Unauthorized();

            var result = await _mediator.Send(new SetDefaultAddressCommand(id, customerId));
            return StatusCode(result.StatusCode, result);
        }
    }
}
