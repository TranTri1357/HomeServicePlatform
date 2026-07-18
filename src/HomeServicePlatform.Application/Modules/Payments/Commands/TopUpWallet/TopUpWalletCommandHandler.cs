using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.TopUpWallet
{
    public class TopUpWalletCommandHandler : IRequestHandler<TopUpWalletCommand, ApiResponse<decimal>>
    {
        private readonly IApplicationDbContext _context;

        public TopUpWalletCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<decimal>> Handle(TopUpWalletCommand request, CancellationToken ct)
        {
            if (request.Amount <= 0)
                throw new BadRequestException("Số tiền nạp phải lớn hơn 0.");

            // 🛡️ Hai ví hệ thống chỉ được biến động bởi bút toán nội bộ, không bao giờ qua API người dùng.
            if (SystemAccounts.IsSystemAccount(request.CustomerId))
                throw new ForbiddenException("Không thể thao tác trực tiếp trên ví hệ thống.");

            // 🛡️ Lặp lại kiểm tra của validator: handler là hàng rào cuối, không phụ thuộc việc
            // pipeline validation có được gắn hay không.
            if (request.Method != PaymentMethod.Momo && request.Method != PaymentMethod.ZaloPay)
                throw new BadRequestException("Chỉ hỗ trợ nạp ví qua MoMo hoặc ZaloPay.");

            var wallet = await WalletLedger.ResolveOneAsync(_context, request.CustomerId, ct);

            // ĐẦU VÀO của dòng tiền: tiền từ bên ngoài đi vào hệ thống nên chỉ có một vế ghi có.
            WalletLedger.Credit(
                wallet,
                WalletTransactionType.TopUp,
                request.Amount,
                referenceId: null,
                now: DateTimeOffset.UtcNow,
                note: $"Nạp qua {GatewayName(request.Method)}");

            await _context.SaveChangesAsync(ct);

            return ApiResponse<decimal>.Success(
                wallet.Balance,
                $"Nạp tiền vào ví qua {GatewayName(request.Method)} thành công.");
        }

        private static string GatewayName(PaymentMethod method)
            => method == PaymentMethod.ZaloPay ? "ZaloPay" : "MoMo";
    }
}
