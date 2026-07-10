using System.Security.Claims;
using HomeServicePlatform.Application.Modules.Commissions.Commands.CreateCommission;
using HomeServicePlatform.Application.Modules.Commissions.Commands.TerminateCommission;
using HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission;
using HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions;
using HomeServicePlatform.Application.Modules.Commissions.Queries.GetCommissionDetail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/commissions")]
    [Authorize(Roles = "Admin,SuperAdmin")] // Đồng nhất quyền với các trang quản trị khác
    public class CommissionManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CommissionManagementController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCommissionsQuery query)
        {
            // 🛡️ 1. Xác thực Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(query);
            return StatusCode(result.StatusCode, result);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetDetail([FromRoute] long id)
        {
            // 🛡️ 2. Xác thực Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            var result = await _mediator.Send(new GetCommissionDetailQuery(id));
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCommissionCommand command)
        {
            // 🛡️ 3. Xác thực Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            // Gợi ý: Nếu trong CreateCommissionCommand của bạn có trường ghi nhận người tạo, hãy map qua:
            // var securedCommand = command with { CreatedBy = adminId };

            var result = await _mediator.Send(command);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateCommissionCommand command)
        {
            // 🛡️ 4. Xác thực Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            // Bảo mật Id-Spoofing: Ép ID từ Route URL vào Command
            // Đồng thời gán adminId vào nếu cấu hình Command của bạn cần trường đối soát (ví dụ: UpdatedBy)
            var securedCommand = command with { CommissionId = id };

            var result = await _mediator.Send(securedCommand);
            return StatusCode(result.StatusCode, result);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Terminate([FromRoute] long id)
        {
            // 🛡️ 5. Xác thực Token ngầm
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("uid");
            if (userIdClaim == null || !long.TryParse(userIdClaim.Value, out long adminId))
            {
                return Unauthorized();
            }

            // Gợi ý bổ sung: Nếu TerminateCommissionCommand hỗ trợ lưu vết lịch sử người thực hiện chấm dứt cấu hình hoa hồng
            // Bạn có thể chỉnh sửa Record Command thành: new TerminateCommissionCommand(id, adminId)
            var result = await _mediator.Send(new TerminateCommissionCommand(id));
            return StatusCode(result.StatusCode, result);
        }
    }
}