using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Commissions.Commands.CreateCommission
{
    public record CreateCommissionCommand(
        long? ServiceId,
        long? TaskerId,
        decimal CommissionRate,
        DateTimeOffset EffectiveFrom,
        DateTimeOffset? EffectiveTo
    ) : IRequest<ApiResponse<long>>;
}
