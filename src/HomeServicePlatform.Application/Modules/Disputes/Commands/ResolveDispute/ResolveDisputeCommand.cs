using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.ResolveDispute
{
    public record ResolveDisputeCommand(
        long DisputeId,            // Ép buộc nhận từ Route URL
        short NewStatus,           // 1: Resolved (Đồng ý bồi hoàn), 2: Rejected (Từ chối khiếu nại)
        string ResolutionNote,     // Nội dung phán quyết của Admin
        decimal? RefundAmount,     // Số tiền hoàn trả cho khách (nếu có)
        int CurrentRowVersion      // Kiểm tra bất đồng bộ tránh 2 Admin duyệt cùng lúc
    ) : IRequest<ApiResponse<bool>>;
}
