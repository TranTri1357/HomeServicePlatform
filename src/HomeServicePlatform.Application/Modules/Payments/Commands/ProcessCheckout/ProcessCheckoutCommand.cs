using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ProcessCheckout
{
    public record CheckoutResponse(
        long PaymentId,
        bool IsPaid,
        string? PaymentUrl
    );

    // Dữ liệu tiếp nhận yêu cầu thanh toán từ Client gửi lên.
    // ⚠️ KHÔNG nhận số tiền từ client (chống giả mạo). Server tự tính từ Booking.FinalAmount;
    //    IsDeposit=true -> chỉ thu cọc 30%, phần còn lại trả khi hoàn thành.
    public record ProcessCheckoutCommand(
        long CustomerId,
        long BookingId,
        bool IsDeposit,
        PaymentMethod Method // 1: SystemWallet, 2: MoMo, 3: ZaloPay, 4: VNPAY, 5: Cash
    ) : IRequest<ApiResponse<CheckoutResponse>>;
}
