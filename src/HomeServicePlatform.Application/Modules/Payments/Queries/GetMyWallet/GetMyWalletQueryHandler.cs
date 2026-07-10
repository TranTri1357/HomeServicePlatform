using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetMyWallet
{
    public class GetMyWalletQueryHandler : IRequestHandler<GetMyWalletQuery, ApiResponse<WalletDto>>
    {
        private const int RecentLimit = 20;
        private readonly IApplicationDbContext _context;

        public GetMyWalletQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<WalletDto>> Handle(GetMyWalletQuery request, CancellationToken ct)
        {
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == request.CustomerId, ct);

            // Chưa có ví thì coi như số dư 0 (ví sẽ được tạo khi nạp lần đầu).
            if (wallet == null)
                return ApiResponse<WalletDto>.Success(new WalletDto(), "Khách hàng chưa có ví, số dư 0.");

            var transactions = await _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(RecentLimit)
                .Select(t => new WalletTransactionDto(
                    t.TransactionId,
                    t.Type,
                    t.Amount,
                    t.BalanceAfter,
                    t.CreatedAt))
                .ToListAsync(ct);

            var dto = new WalletDto
            {
                WalletId = wallet.WalletId,
                Balance = wallet.Balance,
                RecentTransactions = transactions
            };

            return ApiResponse<WalletDto>.Success(dto, "Lấy thông tin ví thành công.");
        }
    }
}
