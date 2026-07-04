using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetPagedPayments;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetPaymentDetail
{
    public record GetPaymentDetailQuery(long PaymentId) : IRequest<ApiResponse<PaymentLookupDto>>;
}
