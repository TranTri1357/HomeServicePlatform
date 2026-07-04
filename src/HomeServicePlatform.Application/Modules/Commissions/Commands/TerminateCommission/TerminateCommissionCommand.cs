using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Commissions.Commands.TerminateCommission
{
    public record TerminateCommissionCommand(long CommissionId) : IRequest<ApiResponse<bool>>;
}
