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
                        dispute.ResolvedAt = now;
                        dispute.UpdatedAt = now;
                        dispute.RowVersion += 1; // Tăng cờ bảo vệ phiên tiếp theo

                        // 4. 🟢 ĐỒNG BỘ DOANH NGHIỆP: Tự động điều chỉnh trạng thái đơn hàng (Booking)
                        var booking = await _context.Bookings
                            .Include(b => b.BookingItems) // RefundExecutor cần để xác định thợ đảm nhận đơn
                            .FirstOrDefaultAsync(b => b.BookingId == dispute.BookingId, ct);

                        if (booking != null)
                        {
                            // Ví dụ: Nếu đồng ý hoàn tiền (NewStatus = 1), chuyển trạng thái đơn sang Đã bồi hoàn/Đóng ca
                            // Nếu từ chối (NewStatus = 2), hoàn trả đơn về trạng thái hoàn thành cũ
                            booking.Status = request.NewStatus == 1 ? BookingStatus.Refund : BookingStatus.Completed;
                            booking.UpdatedAt = now;
                        }

                        // 4b. 💸 BỒI THƯỜNG THEO PHÁN QUYẾT: sàn tự chi từ ví DOANH THU.
                        //
                        //     Trước đây khối này tự cộng thẳng `wallet.Balance += refundAmount` mà
                        //     KHÔNG có vế ghi nợ nào -> tiền sinh ra từ hư không, phá bất biến đối
                        //     soát Σ(mọi ví) = tổng nạp − tổng rút. Nay là bút toán HAI VẾ: ghi nợ
                        //     ví doanh thu, ghi có ví người khiếu nại.
                        //
                        //     Khác với luồng hủy đơn (RefundExecutor giải phóng tiền đang giữ ở ví
                        //     ký quỹ), đây là khoản sàn CHỦ ĐỘNG đền theo phán quyết của admin nên
                        //     luôn lấy từ ví doanh thu, không phụ thuộc đơn đã thu được bao nhiêu.
                        decimal requestedRefund = request.RefundAmount ?? 0m;
                        decimal actualRefunded = 0m;

                        if (request.NewStatus == 1 && requestedRefund > 0m)
                        {
                            if (booking == null)
                            {
                                throw new BadRequestException(
                                    "Không tìm thấy đơn của ca khiếu nại nên không xác định được mức trần bồi thường.");
                            }

                            // 🚧 TRẦN BỒI THƯỜNG = tổng giá trị đơn. Báo lỗi thay vì âm thầm cắt bớt,
                            //    để admin biết chính xác con số vừa nhập là không hợp lệ.
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

                            // Vế ghi NỢ — sàn chi từ ví doanh thu. Thiếu số dư thì Debit ném lỗi,
                            // tuyệt đối không để quỹ âm.
                            WalletLedger.Debit(
                                wallets[SystemAccounts.RevenueUserId],
                                WalletTransactionType.EscrowOut,
                                requestedRefund,
                                dispute.BookingId,
                                now,
                                note: $"Bồi thường khiếu nại đơn BK{dispute.BookingId}",
                                insufficientMessage: "Ví doanh thu của sàn không đủ số dư để chi khoản bồi thường này.");

                            // Vế ghi CÓ — người khiếu nại. Khách thì là hoàn tiền; thợ thì là bồi
                            // thường, nên ghi loại Adjustment để lịch sử ví đọc đúng bản chất.
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

                        // Ghi nhận số tiền THỰC TẾ đã chuyển (0 nếu đóng ca mà không bồi thường).
                        dispute.RefundAmount = actualRefunded;

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
