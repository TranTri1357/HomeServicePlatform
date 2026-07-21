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

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.WithdrawWallet
{
    public class WithdrawWalletCommandHandler : IRequestHandler<WithdrawWalletCommand, ApiResponse<decimal>>
    {
        private readonly IApplicationDbContext _context;

        public WithdrawWalletCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<decimal>> Handle(WithdrawWalletCommand request, CancellationToken ct)
        {
            if (request.Amount <= 0)
                throw new BadRequestException("Số tiền rút phải lớn hơn 0.");

            if (SystemAccounts.IsSystemAccount(request.TaskerId))
                throw new ForbiddenException("Không thể thao tác trực tiếp trên ví hệ thống.");

            var wallet = await WalletLedger.ResolveOneAsync(_context, request.TaskerId, ct);

            var maskedAccount = SensitiveMask.Tail(request.AccountNumber);
            var maskedPhone = SensitiveMask.Tail(request.PhoneNumber);
            var destination = $"{request.BankName.Trim()} {maskedAccount} · SĐT {maskedPhone}";

            WalletLedger.Debit(
                wallet,
                WalletTransactionType.Withdraw,
                request.Amount,
                referenceId: null,
                now: DateTimeOffset.UtcNow,
                note: $"Rút về {destination}",
                insufficientMessage: "Số dư ví không đủ để thực hiện lệnh rút tiền.");

            await _context.SaveChangesAsync(ct);

            return ApiResponse<decimal>.Success(
                wallet.Balance,
                $"Đã chuyển {request.Amount:N0}đ về {destination}.");
        }
    }
}
