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

            // 🛡️ Ví hệ thống chỉ biến động qua bút toán nội bộ, không bao giờ rút qua API.
            if (SystemAccounts.IsSystemAccount(request.TaskerId))
                throw new ForbiddenException("Không thể thao tác trực tiếp trên ví hệ thống.");

            // Ví thợ dùng chung bảng Wallet, khóa theo UserId (== TaskerProfileId).
            var wallet = await WalletLedger.ResolveOneAsync(_context, request.TaskerId, ct);

            // 🔒 Chỉ lưu phần ĐÃ CHE SỐ: đủ để thợ nhận ra tài khoản của mình, không đủ để người
            //    khác tái sử dụng nếu lịch sử giao dịch bị lộ.
            var maskedAccount = SensitiveMask.Tail(request.AccountNumber);
            var maskedPhone = SensitiveMask.Tail(request.PhoneNumber);
            var destination = $"{request.BankName.Trim()} {maskedAccount} · SĐT {maskedPhone}";

            // ĐẦU RA của dòng tiền: tiền rời khỏi hệ thống nên chỉ có một vế ghi nợ.
            // Debit tự chặn nếu số dư không đủ.
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
