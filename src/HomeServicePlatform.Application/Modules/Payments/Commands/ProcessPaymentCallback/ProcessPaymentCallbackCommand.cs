using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ProcessPaymentCallback
{
    public record ProcessPaymentCallbackCommand(
        long PaymentId,
        short NewStatus,              // 1: Success, 2: Failed
        string TransactionCode,        // Mã giao dịch từ cổng đối tác
        int CurrentRowVersion          // Bắt buộc truyền lên để check Optimistic Concurrency Control
    ) : IRequest<ApiResponse<bool>>;
}
