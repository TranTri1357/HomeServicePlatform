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
        private readonly IApplicationDbContext _context;

        public GetTaskerIncomeQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<TaskerIncomeDto>> Handle(GetTaskerIncomeQuery request, CancellationToken ct)
        {
            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

            // Ví của thợ dùng chung bảng Wallet, khóa theo UserId (== TaskerProfileId).
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == request.TaskerId, ct);

            // Chưa có ví => chưa phát sinh thu nhập.
            if (wallet == null)
                return ApiResponse<TaskerIncomeDto>.Success(new TaskerIncomeDto(), "Thợ chưa có ví, số dư 0.");

            var earningQuery = _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.WalletId && t.Type == Earning);

            var totalCount = await earningQuery.CountAsync(ct);
            var totalEarned = await earningQuery.SumAsync(t => (decimal?)t.Amount, ct) ?? 0m;

            var txs = await earningQuery
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.TransactionId,
                    BookingId = t.ReferenceId,
                    Net = t.Amount,
                    t.BalanceAfter,
                    t.CreatedAt
                })
                .ToListAsync(ct);

            // Nạp giá gộp + tên dịch vụ theo từng đơn (chỉ hạng mục của thợ này).
            var bookingIds = txs.Where(t => t.BookingId != null)
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

            var entries = new List<IncomeEntryDto>(txs.Count);
            foreach (var t in txs)
            {
                decimal gross = t.Net;
                string summary = "Đơn hoàn thành";
                if (t.BookingId != null && byBooking.TryGetValue(t.BookingId.Value, out var info))
                {
                    gross = info.Gross;
                    summary = info.Summary;
                }

                entries.Add(new IncomeEntryDto(
                    t.TransactionId,
                    t.BookingId ?? 0,
                    summary,
                    gross,
                    gross - t.Net,
                    t.Net,
                    t.BalanceAfter,
                    t.CreatedAt));
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

        // "Dọn nhà" hoặc "Dọn nhà +2" khi đơn có nhiều dịch vụ.
        private static string BuildSummary(IEnumerable<string> names)
        {
            var distinct = names.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
            if (distinct.Count == 0) return "Đơn hoàn thành";
            return distinct.Count == 1 ? distinct[0] : $"{distinct[0]} +{distinct.Count - 1}";
        }
    }
}
