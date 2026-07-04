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
        string? TransactionCode, // Tìm kiếm theo mã giao dịch
        short? Status,           // Lọc theo trạng thái (0: Chờ, 1: Thành công, 2: Thất bại)
        short? Method,           // Lọc theo phương thức (VNPAY, Momo, Tiền mặt)
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<PaymentLookupDto>>>;
}
