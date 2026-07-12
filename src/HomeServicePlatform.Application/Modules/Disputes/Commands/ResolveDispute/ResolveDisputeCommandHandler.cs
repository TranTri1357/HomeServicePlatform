using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.ResolveDispute
{
    public class ResolveDisputeCommandHandler : IRequestHandler<ResolveDisputeCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public ResolveDisputeCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(ResolveDisputeCommand request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.ResolutionNote))
            {
                throw new BadRequestException("Vui lòng nhập nội dung ghi chú giải quyết tranh chấp.");
            }

            var now = DateTimeOffset.UtcNow;

            if (_context is DbContext efContext)
            {
                var strategy = efContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await efContext.Database.BeginTransactionAsync(ct);
                    try
                    {
                        // 1. Tìm bản ghi tranh chấp cần can thiệp
                        var dispute = await _context.Disputes
                            .FirstOrDefaultAsync(x => x.DisputeId == request.DisputeId, ct);

                        if (dispute == null) throw new NotFoundException($"Không tìm thấy ca tranh chấp số #{request.DisputeId}");

                        // 2. 🛡️ BẢO MẬT: Đóng dấu theo dõi RowVersion chuẩn native để ngăn chặn xung đột ghi đè
                        efContext.Entry(dispute).Property(x => x.RowVersion).OriginalValue = request.CurrentRowVersion;

                        // Chặn nếu ca này đã được giải quyết xong từ trước rồi
                        if (dispute.Status == 1 || dispute.Status == 2)
                        {
                            return ApiResponse<bool>.Success(true, "Ca tranh chấp khiếu nại này đã được xử lý kết toán từ trước.");
                        }

                        // 3. Cập nhật phán quyết của Admin
                        dispute.Status = request.NewStatus;
                        dispute.ResolutionNote = request.ResolutionNote.Trim();
                        dispute.RefundAmount = request.RefundAmount ?? 0;
                        dispute.ResolvedAt = now;
                        dispute.UpdatedAt = now;
                        dispute.RowVersion += 1; // Tăng cờ bảo vệ phiên tiếp theo

                        // 4. 🟢 ĐỒNG BỘ DOANH NGHIỆP: Tự động điều chỉnh trạng thái đơn hàng (Booking)
                        var booking = await _context.Bookings
                            .FirstOrDefaultAsync(b => b.BookingId == dispute.BookingId, ct);

                        if (booking != null)
                        {
                            // Ví dụ: Nếu đồng ý hoàn tiền (NewStatus = 1), chuyển trạng thái đơn sang Đã bồi hoàn/Đóng ca
                            // Nếu từ chối (NewStatus = 2), hoàn trả đơn về trạng thái hoàn thành cũ
                            booking.Status = request.NewStatus == 1 ? BookingStatus.Refund : BookingStatus.Completed;
                            booking.UpdatedAt = now;
                        }

                        // 4b. 💸 HOÀN TIỀN: Admin đồng ý hoàn (NewStatus == 1) và có số tiền hoàn > 0
                        //     -> cộng thẳng vào ví của NGƯỜI KHIẾU NẠI (RaisedById) kèm giao dịch
                        //     loại Refund. Chặn re-resolve ở trên đảm bảo không hoàn 2 lần.
                        decimal refundAmount = request.RefundAmount ?? 0m;
                        if (request.NewStatus == 1 && refundAmount > 0m)
                        {
                            var wallet = await _context.Wallets
                                .FirstOrDefaultAsync(w => w.UserId == dispute.RaisedById, ct);
                            if (wallet == null)
                            {
                                wallet = new Wallet { UserId = dispute.RaisedById, Balance = 0m };
                                _context.Wallets.Add(wallet);
                            }

                            var balanceBefore = wallet.Balance;
                            wallet.Balance += refundAmount;

                            wallet.WalletTransactions.Add(new WalletTransaction
                            {
                                Type = (short)WalletTransactionType.Refund,
                                Amount = refundAmount,
                                BalanceBefore = balanceBefore,
                                BalanceAfter = wallet.Balance,
                                ReferenceId = dispute.BookingId,
                                CreatedAt = now
                            });
                        }

                        // 5. Chốt gộp câu lệnh
                        await _context.SaveChangesAsync(ct);
                        await transaction.CommitAsync(ct);

                        return ApiResponse<bool>.Success(true, "Phán quyết và xử lý đóng ca tranh chấp thành công.");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        await transaction.RollbackAsync(ct);
                        throw new BadRequestException("Dữ liệu tranh chấp đã bị thay đổi bởi một Admin khác trước đó. Vui lòng F5 tải lại trang.");
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync(ct);
                        throw;
                    }
                });
            }

            throw new BadRequestException("Hệ thống lỗi không hỗ trợ bảo mật Transaction.");
        }
    }
}
