using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetPagedPayments
{
    public record PaymentLookupDto(
        long PaymentId,
        long BookingId,
        decimal Amount,
        short Method,
        short Status,
        string? TransactionCode,
        DateTimeOffset? PaidAt,
        DateTimeOffset CreatedAt
    );

    public record GetPagedPaymentsQuery(
        string? TransactionCode,
        short? Status,
        short? Method,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<PaymentLookupDto>>>;
}
