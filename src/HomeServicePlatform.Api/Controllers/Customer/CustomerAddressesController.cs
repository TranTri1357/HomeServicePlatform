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

        [HttpGet]
        public async Task<IActionResult> GetMyAddresses()
        {
            var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(customerIdClaim))
                return Unauthorized(new { message = "Vui lòng đăng nhập để thực hiện." });

            var query = new GetCustomerAddressesQuery
            {
                CustomerId = long.Parse(customerIdClaim)
            };

            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }
    }
}
