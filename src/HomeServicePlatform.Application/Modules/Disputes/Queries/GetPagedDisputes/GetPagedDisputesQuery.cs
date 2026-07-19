using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Disputes.Queries.GetPagedDisputes
{
    public record DisputeLookupDto(
        long DisputeId,
        long BookingId,
        long RaisedById,
        string RaisedByName,
        string Reason,
        short Status,
        string? ResolutionNote,
        decimal? RefundAmount,
        DateTimeOffset? ResolvedAt,
        DateTimeOffset CreatedAt,
        int RowVersion // Trả về để Frontend giữ làm cờ khi gọi lệnh Resolve
    );

    public record GetPagedDisputesQuery(
        short? Status, // Lọc trạng thái (0: Chờ xử lý, 1: Đã hoàn tiền, 2: Khiếu nại bị từ chối)
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<DisputeLookupDto>>>;
}
