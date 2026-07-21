using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Constants;
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
                        var dispute = await _context.Disputes
                            .FirstOrDefaultAsync(x => x.DisputeId == request.DisputeId, ct);

                        if (dispute == null) throw new NotFoundException($"Không tìm thấy ca tranh chấp số #{request.DisputeId}");

                        efContext.Entry(dispute).Property(x => x.RowVersion).OriginalValue = request.CurrentRowVersion;

                        if (dispute.Status == 1 || dispute.Status == 2)
                        {
                            return ApiResponse<bool>.Success(true, "Ca tranh chấp khiếu nại này đã được xử lý kết toán từ trước.");
                        }

                        dispute.Status = request.NewStatus;
                        dispute.ResolutionNote = request.ResolutionNote.Trim();
                        dispute.ResolvedAt = now;
                        dispute.UpdatedAt = now;
                        dispute.RowVersion += 1;

                        var booking = await _context.Bookings
                            .Include(b => b.BookingItems)
                            .FirstOrDefaultAsync(b => b.BookingId == dispute.BookingId, ct);

                        if (booking != null)
                        {
                            booking.Status = request.NewStatus == 1
                                ? BookingStatus.Refund
                                : BookingStatus.DisputeRejected;
                            booking.UpdatedAt = now;
                        }

                        decimal requestedRefund = request.RefundAmount ?? 0m;
                        decimal actualRefunded = 0m;

                        if (request.NewStatus == 1 && requestedRefund > 0m)
                        {
                            if (booking == null)
                            {
                                throw new BadRequestException(
                                    "Không tìm thấy đơn của ca khiếu nại nên không xác định được mức trần bồi thường.");
                            }

                            if (requestedRefund > booking.FinalAmount)
                            {
                                throw new BadRequestException(
                                    $"Số tiền bồi thường ({requestedRefund:N0}đ) không được vượt quá giá trị đơn " +
                                    $"BK{booking.BookingId} ({booking.FinalAmount:N0}đ).");
                            }

                            var wallets = await WalletLedger.ResolveAsync(
                                _context,
                                new[] { dispute.RaisedById, SystemAccounts.RevenueUserId },
                                ct);

                            WalletLedger.Debit(
                                wallets[SystemAccounts.RevenueUserId],
                                WalletTransactionType.EscrowOut,
                                requestedRefund,
                                dispute.BookingId,
                                now,
                                note: $"Bồi thường khiếu nại đơn BK{dispute.BookingId}",
                                insufficientMessage: "Ví doanh thu của sàn không đủ số dư để chi khoản bồi thường này.");

                            var raisedByCustomer = dispute.RaisedById == booking.CustomerId;

                            WalletLedger.Credit(
                                wallets[dispute.RaisedById],
                                raisedByCustomer ? WalletTransactionType.Refund : WalletTransactionType.Adjustment,
                                requestedRefund,
                                dispute.BookingId,
                                now,
                                note: $"Bồi thường khiếu nại đơn BK{dispute.BookingId}");

                            actualRefunded = requestedRefund;
                        }

                        dispute.RefundAmount = actualRefunded;

                        _context.Notifications.Add(request.NewStatus == 1
                            ? NotificationBuilder.Build(
                                dispute.RaisedById,
                                NotificationType.DisputeResolved,
                                "Khiếu nại được chấp nhận",
                                actualRefunded > 0m
                                    ? $"Khiếu nại đơn BK{dispute.BookingId} đã được chấp nhận. " +
                                      $"Số tiền {actualRefunded:N0}đ đã được chuyển vào ví của bạn."
                                    : $"Khiếu nại đơn BK{dispute.BookingId} đã được chấp nhận. {dispute.ResolutionNote}")
                            : NotificationBuilder.Build(
                                dispute.RaisedById,
                                NotificationType.DisputeRejected,
                                "Khiếu nại bị từ chối",
                                $"Khiếu nại đơn BK{dispute.BookingId} không được chấp nhận. Lý do: {dispute.ResolutionNote}"));

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
