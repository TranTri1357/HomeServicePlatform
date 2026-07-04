using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Commissions.Queries.GetCommissionDetail
{
    public record GetCommissionDetailQuery(long CommissionId) : IRequest<ApiResponse<CommissionDto>>;
}
