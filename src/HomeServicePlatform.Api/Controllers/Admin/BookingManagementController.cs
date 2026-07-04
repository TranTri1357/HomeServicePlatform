using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/bookings")]
    [Authorize(Roles = "Admin")]
    // [Authorize(Roles = "Admin,Manager")] // Gác cổng bảo mật riêng cho Admin tại đây
    public class BookingManagementController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingManagementController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// API Admin/Tổng đài viên đặt hộ lịch hẹn dịch vụ cho Khách hàng
        /// </summary>
        /// <param name="command">Dữ liệu gộp từ CMS gửi lên (Admin tự điền CustomerId của khách)</param>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CreateBookingResponse>), StatusCodes.Status201Created)]
        public async Task<IActionResult> AdminCreateBooking([FromBody] CreateBookingCommand command)
        {
            // 🟢 Gọi MediatR Send: Tận dụng lại 100% Handler cũ của bạn mà không cần viết lại code logic
            var result = await _mediator.Send(command);

            return StatusCode(StatusCodes.Status201Created, result);
        }
    }
}
