using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

            // Ví thợ dùng chung bảng Wallet, khóa theo UserId (== TaskerProfileId).
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.TaskerId, ct);

            if (wallet == null || wallet.Balance < request.Amount)
                throw new BadRequestException("Số dư ví không đủ để thực hiện lệnh rút tiền.");

            var balanceBefore = wallet.Balance;
            wallet.Balance -= request.Amount;

            // Ghi lịch sử giao dịch (ghi nợ). Amount lưu dương; dấu (-) do frontend
            // hiển thị theo loại giao dịch.
            wallet.WalletTransactions.Add(new WalletTransaction
            {
                Type = (short)WalletTransactionType.Withdraw,
                Amount = request.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync(ct);

            return ApiResponse<decimal>.Success(wallet.Balance, "Rút tiền thành công.");
        }
    }
}
