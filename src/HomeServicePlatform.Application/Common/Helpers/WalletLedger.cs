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
    /// <summary>
    /// Nơi DUY NHẤT được phép làm biến động số dư ví. Gom lại để mọi bút toán đều tuân thủ
    /// cùng một bộ quy tắc: số tiền luôn dương, làm tròn đúng thang decimal(18,2) của cột,
    /// ghi đầy đủ BalanceBefore/BalanceAfter và không bao giờ để ví âm.
    ///
    /// Mô hình dòng tiền là GHI SỔ KÉP: trừ một trong hai đầu ranh giới (khách nạp tiền vào,
    /// thợ rút tiền ra), mỗi sự kiện đều gồm các vế cộng/trừ có tổng bằng 0. Bất biến kiểm
    /// chứng: Σ(mọi ví) = tổng đã nạp − tổng đã rút.
    /// </summary>
    public static class WalletLedger
    {
        /// <summary>Thang số của cột "balance"/"amount" — làm tròn tại đây để hai vế không lệch nhau vì DB tự cắt số.</summary>
        private const int MoneyScale = 2;

        /// <summary>
        /// Nạp một lượt nhiều ví theo UserId trong MỘT truy vấn (thay vì mỗi ví một vòng gọi DB).
        /// Ví đã nằm trong ChangeTracker được tái sử dụng — quan trọng khi cùng một ví (điển hình
        /// là ví ký quỹ) bị chạm nhiều lần trong cùng một luồng xử lý.
        ///
        /// Ví người dùng thiếu thì tạo mới (lazy); ví HỆ THỐNG thiếu thì báo lỗi rõ ràng, vì
        /// chúng phải tồn tại sẵn qua seed — tự tạo sẽ đẻ ví mồ côi và làm sai đối soát.
        /// </summary>
        public static async Task<IReadOnlyDictionary<long, Wallet>> ResolveAsync(
            IApplicationDbContext context,
            IEnumerable<long> userIds,
            CancellationToken ct)
        {
            var ids = userIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<long, Wallet>();

            // 1. Ưu tiên các ví EF đang theo dõi (vừa tạo hoặc vừa đọc ở bước trước).
            var resolved = context.Wallets.Local
                .Where(w => ids.Contains(w.UserId))
                .GroupBy(w => w.UserId)
                .ToDictionary(g => g.Key, g => g.First());

            // 2. Phần còn thiếu: một truy vấn duy nhất.
            var missing = ids.Where(id => !resolved.ContainsKey(id)).ToList();
            if (missing.Count > 0)
            {
                var loaded = await context.Wallets
                    .Where(w => missing.Contains(w.UserId))
                    .ToListAsync(ct);

                foreach (var wallet in loaded)
                    resolved[wallet.UserId] = wallet;
            }

            // 3. Vẫn thiếu -> tạo mới cho người dùng, báo lỗi cho tài khoản hệ thống.
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

        /// <summary>Nạp đúng một ví (bọc lại <see cref="ResolveAsync"/> cho các luồng chỉ chạm một ví).</summary>
        public static async Task<Wallet> ResolveOneAsync(
            IApplicationDbContext context,
            long userId,
            CancellationToken ct)
        {
            var wallets = await ResolveAsync(context, new[] { userId }, ct);
            return wallets[userId];
        }

        /// <summary>Ghi CÓ: cộng tiền vào ví và lưu vết biến động.</summary>
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

        /// <summary>
        /// Ghi NỢ: trừ tiền khỏi ví. Chặn số dư âm — với ví ký quỹ đây là lằn ranh an toàn cuối
        /// cùng, vì chi vượt số đang giữ nghĩa là hệ thống đang trả bằng tiền của đơn khác.
        /// </summary>
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

        // Amount luôn lưu DƯƠNG (bảng có check "amount <> 0"); dấu do loại giao dịch quyết định
        // và do frontend hiển thị.
        private static void Record(
            Wallet wallet,
            WalletTransactionType type,
            decimal amount,
            decimal balanceBefore,
            long? referenceId,
            DateTimeOffset now,
            string? note)
        {
            // Thêm qua navigation để EF tự gán WalletId, kể cả với ví vừa được tạo trong cùng lượt lưu.
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

            // Bảng wallet_transactions có ràng buộc "amount <> 0": ghi bút toán 0 đồng sẽ vỡ DB.
            if (value <= 0m)
                throw new BadRequestException("Số tiền của giao dịch ví phải lớn hơn 0.");

            return value;
        }

        // Cột note giới hạn 200 ký tự — cắt chủ động thay vì để DB ném lỗi.
        private static string? Truncate(string? note)
            => string.IsNullOrWhiteSpace(note)
                ? null
                : note.Trim().Length <= 200 ? note.Trim() : note.Trim()[..200];
    }
}
