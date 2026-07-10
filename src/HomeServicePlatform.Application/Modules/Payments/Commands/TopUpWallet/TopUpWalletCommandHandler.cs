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

            // Tạo ví lười (lazy) nếu khách chưa có ví.
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == request.CustomerId, ct);

            if (wallet == null)
            {
                wallet = new Wallet { UserId = request.CustomerId, Balance = 0m };
                _context.Wallets.Add(wallet);
            }

            var balanceBefore = wallet.Balance;
            wallet.Balance += request.Amount;

            // Thêm qua navigation để EF tự gán WalletId (kể cả ví vừa tạo).
            wallet.WalletTransactions.Add(new WalletTransaction
            {
                Type = (short)WalletTransactionType.TopUp,
                Amount = request.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                CreatedAt = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync(ct);

            return ApiResponse<decimal>.Success(wallet.Balance, "Nạp tiền vào ví thành công.");
        }
    }
}
