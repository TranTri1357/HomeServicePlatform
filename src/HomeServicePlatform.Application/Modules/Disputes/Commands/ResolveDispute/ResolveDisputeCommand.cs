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
        long DisputeId,
        short NewStatus,
        string ResolutionNote,
        decimal? RefundAmount,
        int CurrentRowVersion
    ) : IRequest<ApiResponse<bool>>;
}
