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

            // Ví của thợ dùng chung bảng Wallet, khóa theo UserId (== TaskerProfileId).
            var wallet = await _context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.UserId == request.TaskerId, ct);

            // Chưa có ví => chưa phát sinh thu nhập.
            if (wallet == null)
                return ApiResponse<TaskerIncomeDto>.Success(new TaskerIncomeDto(), "Thợ chưa có ví, số dư 0.");

            // Toàn bộ giao dịch của ví thợ (thu nhập, rút tiền, điều chỉnh...).
            var allQuery = _context.WalletTransactions
                .AsNoTracking()
                .Where(t => t.WalletId == wallet.WalletId);

            var totalCount = await allQuery.CountAsync(ct);
            // "Tổng đã nhận" chỉ tính các giao dịch thu nhập (Earning).
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

            // Nạp giá gộp + tên dịch vụ chỉ cho các dòng thu nhập (Earning) có đơn.
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

            var entries = new List<IncomeEntryDto>(txs.Count);
            foreach (var t in txs)
            {
                // Chỉ dòng thu nhập (Earning) mới có phần bóc tách gộp → hoa hồng → thực nhận.
                if (t.Type == Earning)
                {
                    decimal gross = t.Net;
                    string summary = "Đơn hoàn thành";
                    if (t.BookingId != null && byBooking.TryGetValue(t.BookingId.Value, out var info))
                    {
                        gross = info.Gross;
                        summary = info.Summary;
                    }

                    entries.Add(new IncomeEntryDto(
                        t.TransactionId, t.Type, t.BookingId ?? 0, summary,
                        gross, gross - t.Net, t.Net, t.BalanceAfter, t.CreatedAt));
                }
                else
                {
                    // Rút tiền / điều chỉnh...: không có gộp/hoa hồng, chỉ số tiền giao dịch.
                    entries.Add(new IncomeEntryDto(
                        t.TransactionId, t.Type, t.BookingId ?? 0, NonEarningLabel(t.Type),
                        0m, 0m, t.Net, t.BalanceAfter, t.CreatedAt));
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

        // "Dọn nhà" hoặc "Dọn nhà +2" khi đơn có nhiều dịch vụ.
        // Nhãn hiển thị cho các giao dịch không phải thu nhập.
        private static string NonEarningLabel(short type) => type switch
        {
            Withdraw => "Rút tiền về tài khoản",
            Adjustment => "Điều chỉnh số dư",
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
