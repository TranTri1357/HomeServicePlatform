using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.CreatePayment
{
    public record CreatePaymentCommand(
        long BookingId,
        decimal Amount,
        short Method
    ) : IRequest<ApiResponse<long>>;
}
