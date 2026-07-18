using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
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

            // Lấy dữ liệu thô rồi dựng mô tả trong bộ nhớ (string nội suy không dịch được sang SQL).
            var raw = await _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.WalletId)
                .OrderByDescending(t => t.CreatedAt)
                .Take(RecentLimit)
                .Select(t => new
                {
                    t.TransactionId,
                    t.Type,
                    t.Amount,
                    t.BalanceAfter,
                    t.ReferenceId,
                    t.Note,
                    t.CreatedAt
                })
                .ToListAsync(ct);

            var transactions = raw
                .Select(t => new WalletTransactionDto(
                    t.TransactionId,
                    t.Type,
                    t.Amount,
                    t.BalanceAfter,
                    t.ReferenceId,
                    BuildDescription(t.Type, t.ReferenceId, t.Note),
                    t.CreatedAt))
                .ToList();

            var dto = new WalletDto
            {
                WalletId = wallet.WalletId,
                Balance = wallet.Balance,
                RecentTransactions = transactions
            };

            return ApiResponse<WalletDto>.Success(dto, "Lấy thông tin ví thành công.");
        }

        // Mô tả rõ lý do biến động số dư ví khách + số đơn liên quan (nếu có).
        // Ưu tiên Note đã ghi lúc tạo giao dịch vì nó cụ thể hơn (vd "Nạp qua MoMo").
        private static string BuildDescription(short type, long? bookingId, string? note)
        {
            if (!string.IsNullOrWhiteSpace(note)) return note;

            var bk = bookingId.HasValue ? $" đơn BK{bookingId.Value}" : string.Empty;
            return type switch
            {
                (short)WalletTransactionType.TopUp => "Nạp tiền vào ví",
                (short)WalletTransactionType.Payment => $"Thanh toán{bk}",
                (short)WalletTransactionType.Refund => bookingId.HasValue
                    ? $"Hoàn tiền{bk}"
                    : "Hoàn tiền",
                (short)WalletTransactionType.Adjustment => bookingId.HasValue
                    ? $"Bồi thường{bk}"
                    : "Điều chỉnh số dư",
                (short)WalletTransactionType.Earning => $"Thu nhập{bk}",
                (short)WalletTransactionType.Withdraw => "Rút tiền về tài khoản",
                _ => "Giao dịch ví"
            };
        }
    }
}
