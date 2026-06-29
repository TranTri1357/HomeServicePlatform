using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking;
using HomeServicePlatform.Application.Modules.Booking.Commands.CompleteWork;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartMoving;
using HomeServicePlatform.Application.Modules.Booking.Commands.StartWorking;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Tasker
{
    [ApiController]
    [Route("api/tasker/bookings")]
    // 🟢 Đăng ký gộp bộ bẫy lỗi 400/500 của Middleware tại đây để các hàm bên dưới không phải viết lại
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Bước 2: Thợ bấm XÁC NHẬN đơn hàng khi có thông báo lịch đặt mới
        /// </summary>
        [HttpPut("{id}/accept")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Accept(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new AcceptBookingCommand(id, body.TaskerId));
            return Ok(result); // Nếu gãy lỗi logic, Middleware sẽ tự tóm lấy, không chạy xuống dòng này
        }

        /// <summary>
        /// Bước 3: Thợ thông báo bắt đầu XUẤT PHÁT di chuyển đến nhà khách hàng
        /// </summary>
        [HttpPut("{id}/start-moving")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartMoving(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new StartMovingCommand(id, body.TaskerId));
            return Ok(result);
        }

        /// <summary>
        /// Bước 4: Thợ đã đến nơi và bấm BẮT ĐẦU THỰC HIỆN công việc dịch vụ
        /// </summary>
        [HttpPut("{id}/start-working")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> StartWorking(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new StartWorkingCommand(id, body.TaskerId));
            return Ok(result);
        }

        /// <summary>
        /// Bước 5: Thợ làm xong hoàn toàn công việc, bấm gửi yêu cầu CHỜ THANH TOÁN
        /// </summary>
        [HttpPut("{id}/complete-work")]
        [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
        public async Task<IActionResult> CompleteWork(long id, [FromBody] TaskerActionBody body)
        {
            var result = await _mediator.Send(new CompleteWorkCommand(id, body.TaskerId));
            return Ok(result);
        }
    }

    /// <summary>
    /// Đối tượng Request Body tinh gọn truyền lên từ App ứng dụng của Thợ.
    /// Bạn có thể giữ ở đây hoặc di chuyển sang file riêng tùy ý.
    /// </summary>
    public record TaskerActionBody(long TaskerId);
}
