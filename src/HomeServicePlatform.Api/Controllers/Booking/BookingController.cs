using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HomeServicePlatform.Api.Controllers.Booking
{
    [ApiController]
    [Route("api/customer/bookings")]
    public class BookingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BookingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// API đặt lịch chuyên nghiệp dành cho App Khách hàng
        /// </summary>
        /// <param name="command">Thông tin yêu cầu đặt lịch từ Client</param>
        /// <returns>Cấu trúc ApiResponse tiêu chuẩn chứa thông tin đơn hàng vừa tạo</returns>
        [HttpPost]
       
        public async Task<IActionResult> Create([FromBody] CreateBookingCommand command)
        {
            // 1. MediatR tự kích hoạt Pipeline Validation chặn dữ liệu rác đầu vào.
            var result = await _mediator.Send(command);

            // 2. Trả về HTTP 201 Created kèm bọc khối dữ liệu đồng nhất toàn hệ thống.
            // result hiện tại đã là một đối tượng ApiResponse<CreateBookingResponse> từ Handler trả ra
            return StatusCode(201, result);
        }
    }
}
