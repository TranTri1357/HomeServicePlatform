using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Payments.Commands.ProcessCheckout;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Customer
{
    [ApiController]
    [Route("api/payments")] // Định nghĩa tuyến đường gọi API tổng
    public class ClientPaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        public ClientPaymentController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// API Thanh toán hóa đơn đa phương thức linh hoạt (Ví hệ thống, MoMo, ZaloPay, Tiền mặt)
        /// </summary>
        /// <param name="command">Thông tin đóng gói đơn hàng và phương thức thanh toán</param>
        [HttpPost("checkout")]
        [ProducesResponseType(typeof(ApiResponse<CheckoutResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] ProcessCheckoutCommand command)
        {
            if (command == null)
            {
                return BadRequest("Dữ liệu cấu trúc thanh toán gửi lên không được phép để trống.");
            }

            // Bắn dữ liệu qua MediatR Pipeline để kích hoạt ProcessCheckoutCommandHandler xử lý
            var result = await _mediator.Send(command);

            // Trả về dữ liệu chuẩn JSON kèm HttpStatusCode động (200, 400, 404, 500)
            return StatusCode(result.StatusCode, result);
        }
    }
}
