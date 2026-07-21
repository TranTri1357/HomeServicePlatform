using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerIncome
{
    public class GetTaskerIncomeQueryHandler
        : IRequestHandler<GetTaskerIncomeQuery, ApiResponse<TaskerIncomeDto>>
    {
        private const short Earning = (short)WalletTransactionType.Earning;
        private const short Withdraw = (short)WalletTransactionType.Withdraw;
        private const short Adjustment = (short)WalletTransactionType.Adjustment;
        private readonly IApplicationDbContext _context;

        public GetTaskerIncomeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerIncomeDto>> Handle(GetTaskerIncomeQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == request.TaskerId, ct);

            if (wallet == null)
                return ApiResponse<TaskerIncomeDto>.Success(new TaskerIncomeDto(), "Thợ chưa có ví, số dư 0.");

            var allQuery = _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.WalletId);

            var totalCount = await allQuery.CountAsync(ct);
            var totalEarned = await allQuery
                .Where(t => t.Type == Earning)
                .SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

            var txs = await allQuery
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.TransactionId,
                    t.Type,
                    BookingId = t.ReferenceId,
                    Net = t.Amount,
                    t.BalanceAfter,
                    t.CreatedAt
                })
                .ToListAsync(ct);

            var bookingIds = txs.Where(t => t.Type == Earning && t.BookingId != null)
                                .Select(t => t.BookingId!.Value)
                                .Distinct()
                                .ToList();

            var items = await _context.BookingItems
                .AsNoTracking()
                .Where(bi => bi.TaskerId == request.TaskerId && bookingIds.Contains(bi.BookingId))
                .Select(bi => new { bi.BookingId, bi.TotalPrice, ServiceName = bi.Service.Name })
                .ToListAsync(ct);

            var byBooking = items
                .GroupBy(i => i.BookingId)
                .ToDictionary(
                    g => g.Key,
                    g => new
                    {
                        Gross = g.Sum(x => x.TotalPrice),
                        Summary = BuildSummary(g.Select(x => x.ServiceName))
                    });

            var heldByBooking = await _context.Payments
                .AsNoTracking()
                .Where(p => bookingIds.Contains(p.BookingId) && p.Status == (short)PaymentStatus.Paid)
                .GroupBy(p => p.BookingId)
                .Select(g => new { BookingId = g.Key, Held = g.Sum(x => x.Amount) })
                .ToDictionaryAsync(x => x.BookingId, x => x.Held, ct);

            var entries = new List<IncomeEntryDto>(txs.Count);
            foreach (var t in txs)
            {
                if (t.Type == Earning)
                {
                    decimal gross = t.Net;
                    string summary = "Đơn hoàn thành";
                    if (t.BookingId != null && byBooking.TryGetValue(t.BookingId.Value, out var info))
                    {
                        gross = info.Gross;
                        summary = info.Summary;
                    }

                    decimal held = t.BookingId != null && heldByBooking.TryGetValue(t.BookingId.Value, out var h) ? h : 0m;
                    decimal commission = Math.Max(0m, held - t.Net);
                    decimal cash = Math.Max(0m, gross - held);

                    entries.Add(new IncomeEntryDto(
                        t.TransactionId, t.Type, t.BookingId ?? 0, summary,
                        gross, commission, t.Net, held, cash, t.BalanceAfter, t.CreatedAt));
                }
                else
                {
                    entries.Add(new IncomeEntryDto(
                        t.TransactionId, t.Type, t.BookingId ?? 0, NonEarningLabel(t.Type, t.BookingId),
                        0m, 0m, t.Net, 0m, 0m, t.BalanceAfter, t.CreatedAt));
                }
            }

            var dto = new TaskerIncomeDto
            {
                Balance = wallet.Balance,
                TotalEarned = totalEarned,
                TotalCount = totalCount,
                Entries = entries
            };

            return ApiResponse<TaskerIncomeDto>.Success(dto, "Lấy lịch sử thu nhập của thợ thành công.");
        }

        private static string NonEarningLabel(short type, long? bookingId) => type switch
        {
            Withdraw => "Rút tiền về tài khoản",
            Adjustment => bookingId.HasValue ? "Đền phí hủy đơn" : "Điều chỉnh số dư",
            (short)WalletTransactionType.Refund => "Hoàn tiền",
            _ => "Giao dịch ví"
        };

        private static string BuildSummary(IEnumerable<string> names)
        {
            var distinct = names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
            if (distinct.Count == 0) return "Đơn hoàn thành";
            return distinct.Count == 1 ? distinct[0] : $"{distinct[0]} +{distinct.Count - 1}";
        }
    }
}
