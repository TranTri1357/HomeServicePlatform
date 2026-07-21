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

    public record ProcessCheckoutCommand(
        long CustomerId,
        long BookingId,
        bool IsDeposit,
        PaymentMethod Method
    ) : IRequest<ApiResponse<CheckoutResponse>>;
}
