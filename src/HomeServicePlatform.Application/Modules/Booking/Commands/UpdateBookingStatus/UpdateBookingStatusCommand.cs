using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.UpdateBookingStatus
{
    public record UpdateBookingStatusCommand(
        long BookingId,            // Nhận từ Route URL
        short NewStatus,           // Trạng thái mới hướng tới
        long ChangedBy,            // ID Admin hoặc Khách hàng thực hiện
        int CurrentRowVersion      // Bảo mật Concurrency chống ghi đè dữ liệu cũ
    ) : IRequest<ApiResponse<bool>>;
}
