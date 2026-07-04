using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions
{
    public record CommissionDto(
        long CommissionId,
        long? ServiceId,
        string? ServiceName,
        long? TaskerId,
        string? TaskerName,
        decimal CommissionRate,
        DateTimeOffset EffectiveFrom,
        DateTimeOffset? EffectiveTo,
        bool IsActive // Đơn vị nào có effective_to > hiện tại hoặc null thì là đang hoạt động
    );

    public record GetAllCommissionsQuery(
        string? SearchTerm, // Tìm theo tên thợ hoặc tên dịch vụ
        int PageIndex = 1,
        int PageSize = 10
    ) : IRequest<ApiResponse<PagedResult<CommissionDto>>>;
}
