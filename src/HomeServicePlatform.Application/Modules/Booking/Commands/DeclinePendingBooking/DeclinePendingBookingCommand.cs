using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclinePendingBooking
{
    /// <summary>
    /// Thợ TỪ CHỐI một đơn thường CHƯA NHẬN (Pending). TaskerId do controller đè từ Token.
    ///
    /// <para>Khác hẳn <c>CancelBookingCommand</c> — đó là thợ BỎ đơn ĐÃ NHẬN, có phạt độ tin cậy.
    /// Từ chối là quyền bình thường của thợ (bận, sai chuyên môn, khách quá xa), nên KHÔNG tăng
    /// CancelCount và KHÔNG đẩy thợ tới ngưỡng khóa tài khoản.</para>
    ///
    /// <para>Khách đã thanh toán trước khi thợ thấy đơn, nên từ chối phải HOÀN 100%.</para>
    /// </summary>
    public record DeclinePendingBookingCommand(
        long BookingId,
        string DeclineReason,
        long TaskerId = 0
    ) : IRequest<ApiResponse<bool>>;
}
