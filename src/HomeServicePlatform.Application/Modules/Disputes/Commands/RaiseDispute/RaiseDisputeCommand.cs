using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.RaiseDispute
{
    public record RaiseDisputeCommand(
        long BookingId,
        long RaisedById, // ID của người gửi khiếu nại (Khách hàng hoặc Thợ)
        string Reason
    ) : IRequest<ApiResponse<long>>;
}
