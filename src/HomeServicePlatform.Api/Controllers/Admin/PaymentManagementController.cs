using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission;
using HomeServicePlatform.Application.Modules.Payments.Commands.CreatePayment;
using HomeServicePlatform.Application.Modules.Payments.Commands.ProcessPaymentCallback;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetPagedPayments;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetPaymentDetail;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/payments")]
    public class PaymentManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PaymentManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentLookupDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetPagedPaymentsQuery query)
        {
            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(ApiResponse<PaymentLookupDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDetail([FromRoute] long id)
        {
            var result = await _mediator.Send(new GetPaymentDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
        {
            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}")] // 🟢 Định nghĩa rõ ID tài nguyên nằm trên tuyến đường URL
        public async Task<IActionResult> UpdatePaymentStatus([FromRoute] long id, [FromBody] ProcessPaymentCallbackCommand command)
        {
            if (command == null)
            {
                return BadRequest("Dữ liệu callback không được để trống.");
            }

            // 🛡️ CHỐNG HACK (Id-Spoofing): Ép buộc lấy ID an toàn từ URL đè xuống Body
            var securedCommand = command with { PaymentId = id };

            // 🟢 Gửi securedCommand đã được đè ID sang cho MediatR xử lý
            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}
