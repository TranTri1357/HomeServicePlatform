using System.Security.Claims;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetPagedPayments;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetPaymentDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/payments")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class PaymentManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PaymentManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentLookupDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] GetPagedPaymentsQuery query)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetDetail([FromRoute] long id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetPaymentDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}