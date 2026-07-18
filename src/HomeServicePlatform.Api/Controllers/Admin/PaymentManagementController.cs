using System.Security.Claims;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission;
using HomeServicePlatform.Application.Modules.Payments.Commands.CreatePayment;
using HomeServicePlatform.Application.Modules.Payments.Commands.ProcessPaymentCallback;
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
    [Authorize(Roles = "Admin,SuperAdmin")] // Đồng nhất quyền với các trang quản trị khác
    public class PaymentManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PaymentManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PaymentLookupDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll([FromQuery] GetPagedPaymentsQuery query)
        {
            // 🛡️ 1. Xác thực Token ngầm cho lệnh lấy danh sách thanh toán
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
            // 🛡️ 2. Xác thực Token ngầm cho lệnh lấy chi tiết thanh toán
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetPaymentDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<long>), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
        {
            // 🛡️ 3. Xác thực Token ngầm cho lệnh tạo bản ghi thanh toán
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdatePaymentStatus([FromRoute] long id, [FromBody] ProcessPaymentCallbackCommand command)
        {
            if (command == null)
            {
                return BadRequest("Dữ liệu callback không được để trống.");
            }

            // 🛡️ 4. Xác thực Token ngầm cho lệnh cập nhật/duyệt trạng thái giao dịch
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            // 🛡️ CHỐNG HACK (Id-Spoofing): Ép buộc lấy ID an toàn từ URL đè xuống Body
            // Bạn có thể gán thêm adminId vào trường ChangedBy/UpdatedBy nếu Command có hỗ trợ đối soát vết
            var securedCommand = command with { PaymentId = id };

            // 🟢 Gửi securedCommand đã được đè ID sang cho MediatR xử lý
            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }
    }
}