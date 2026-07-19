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
    /// <summary>
    /// Tra cứu giao dịch thanh toán cho trang quản trị.
    /// </summary>
    /// <remarks>
    /// 🔒 CHỈ ĐỌC — có chủ đích. Admin KHÔNG được tạo hay sửa tay trạng thái giao dịch.
    /// Mọi thay đổi dòng tiền phải đi qua đúng một trong hai đường có ghi vết đầy đủ:
    ///   • Cổng thanh toán      -> ConfirmMockPaymentCommandHandler (ghi ví ký quỹ EscrowIn, notify thợ)
    ///   • Khiếu nại/hoàn tiền  -> ResolveDisputeCommandHandler / RefundExecutor
    /// Trước đây ở đây có POST (tạo Payment tùy ý) và PUT (duyệt tay trạng thái). Cả hai đều
    /// không được frontend sử dụng, bỏ qua ví ký quỹ và giải ngân hoa hồng, lại còn gán thẳng
    /// Booking sang Completed (vượt mặt state machine) nên đã được gỡ bỏ.
    /// </remarks>
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
    }
}