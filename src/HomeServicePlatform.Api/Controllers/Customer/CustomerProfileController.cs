using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Customer.Queries.GetCustomerProfile;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/customer-profile")]
    public class CustomerProfileController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerProfileController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet("{customerId:long}")]
        [ProducesResponseType(typeof(ApiResponse<CustomerProfileDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfile([FromRoute] long customerId)
        {
            var result = await _mediator.Send(new GetCustomerProfileQuery(customerId));
            return StatusCode(result.StatusCode, result);
        }
    }
}
