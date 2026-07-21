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
        int RowVersion
    );

    public record GetPagedDisputesQuery(
        short? Status,
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<DisputeLookupDto>>>;
}
