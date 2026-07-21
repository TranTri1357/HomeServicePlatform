using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Common.Helpers
{
    public static class WalletLedger
    {
        private const int MoneyScale = 2;

        public static async Task<IReadOnlyDictionary<long, Wallet>> ResolveAsync(
            IApplicationDbContext context,
            IEnumerable<long> userIds,
            CancellationToken ct)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<long, Wallet>();

            var resolved = context.Wallets.Local
                .Where(w => ids.Contains(w.UserId))
                .GroupBy(w => w.UserId)
                .ToDictionary(g => g.Key, g => g.First());

            var missing = ids.Where(id => !resolved.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                var loaded = await context.Wallets
                    .Where(w => missing.Contains(w.UserId))
                    .ToListAsync(ct);

                foreach (var wallet in loaded)
                    resolved[wallet.UserId] = wallet;
            }

            foreach (var id in ids.Where(id => !resolved.ContainsKey(id)))
            {
                if (SystemAccounts.IsSystemAccount(id))
                {
                    throw new BadRequestException(
                        "Chưa khởi tạo ví hệ thống (ký quỹ / doanh thu). Hãy chạy migration để seed dữ liệu nền.");
                }

                var created = new Wallet { UserId = id, Balance = 0m };
                context.Wallets.Add(created);
                resolved[id] = created;
            }

            return resolved;
        }

        public static async Task<Wallet> ResolveOneAsync(
            IApplicationDbContext context,
            long userId,
            CancellationToken ct)
        {
            var wallets = await ResolveAsync(context, new[] { userId }, ct);
            return wallets[userId];
        }

        public static void Credit(
            Wallet wallet,
            WalletTransactionType type,
            decimal amount,
            long? referenceId,
            DateTimeOffset now,
            string? note = null)
        {
            var value = Normalize(amount);

            var balanceBefore = wallet.Balance;
            wallet.Balance = balanceBefore + value;

            Record(wallet, type, value, balanceBefore, referenceId, now, note);
        }

        public static void Debit(
            Wallet wallet,
            WalletTransactionType type,
            decimal amount,
            long? referenceId,
            DateTimeOffset now,
            string? note = null,
            string? insufficientMessage = null)
        {
            var value = Normalize(amount);

            if (wallet.Balance < value)
            {
                throw new BadRequestException(insufficientMessage
                    ?? "Số dư ví không đủ để thực hiện giao dịch này.");
            }

            var balanceBefore = wallet.Balance;
            wallet.Balance = balanceBefore - value;

            Record(wallet, type, value, balanceBefore, referenceId, now, note);
        }

        private static void Record(
            Wallet wallet,
            WalletTransactionType type,
            decimal amount,
            decimal balanceBefore,
            long? referenceId,
            DateTimeOffset now,
            string? note)
        {
            wallet.WalletTransactions.Add(new WalletTransaction
            {
                Type = (short)type,
                Amount = amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = wallet.Balance,
                ReferenceId = referenceId,
                Note = Truncate(note),
                CreatedAt = now
            });
        }

        private static decimal Normalize(decimal amount)
        {
            var value = Math.Round(amount, MoneyScale, MidpointRounding.AwayFromZero);

            if (value <= 0m)
                throw new BadRequestException("Số tiền của giao dịch ví phải lớn hơn 0.");

            return value;
        }

        private static string? Truncate(string? note)
            => string.IsNullOrWhiteSpace(note)
                ? null
                : note.Trim().Length <= 200 ? note.Trim() : note.Trim()[..200];
    }
}
