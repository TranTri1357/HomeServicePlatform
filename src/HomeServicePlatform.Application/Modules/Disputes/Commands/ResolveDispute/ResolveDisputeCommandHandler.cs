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

                        // 4b. 💸 HOÀN TIỀN — dùng CHUNG RefundExecutor với luồng khách/thợ hủy đơn.
                        //
                        //     Trước đây khối này tự cộng thẳng `wallet.Balance += refundAmount` mà
                        //     KHÔNG có vế ghi nợ nào -> tiền sinh ra từ hư không, phá bất biến đối
                        //     soát Σ(mọi ví) = tổng nạp − tổng rút. RefundExecutor giải phóng đúng
                        //     khoản đang giữ ở ví ký quỹ (đơn đã tất toán thì ví doanh thu bù),
                        //     chặn trần theo số THỰC THU, tạo bản ghi Refund và idempotent sẵn.
                        decimal requestedRefund = request.RefundAmount ?? 0m;
                        decimal actualRefunded = 0m;

                        if (request.NewStatus == 1 && requestedRefund > 0m)
                        {
                            if (booking == null)
                                throw new BadRequestException("Không tìm thấy đơn hàng của ca khiếu nại nên không thể hoàn tiền.");

                            // Người khiếu nại là KHÁCH -> hoàn cho khách. Là THỢ -> ghi thành khoản
                            // bồi thường cho thợ (RefundExecutor không khấu hoa hồng khoản này).
                            var raisedByCustomer = dispute.RaisedById == booking.CustomerId;

                            var outcome = await RefundExecutor.IssueRefundAsync(
                                _context,
                                booking,
                                refundAmount: raisedByCustomer ? requestedRefund : 0m,
                                penaltyAmount: raisedByCustomer ? 0m : requestedRefund,
                                RefundInitiator.Admin,
                                $"Hoàn theo phán quyết khiếu nại #{dispute.DisputeId}: {dispute.ResolutionNote}",
                                now,
                                ct);

                            actualRefunded = outcome.RefundedToCustomer + outcome.CompensatedToTasker;

                            // Không im lặng bỏ qua: nếu đơn chưa thu được đồng nào qua hệ thống
                            // (vd đơn tiền mặt) hoặc đã hoàn trước đó thì không có tiền để chuyển.
                            // Báo rõ để admin biết, thay vì ghi nhận một con số không có thật.
                            if (!outcome.Executed || actualRefunded <= 0m)
                            {
                                throw new BadRequestException(
                                    "Không thể hoàn tiền cho đơn này: đơn chưa thu được khoản nào qua hệ thống, " +
                                    "hoặc đã được hoàn trước đó. Đặt số tiền hoàn = 0 nếu muốn đóng ca khiếu nại mà không hoàn tiền.");
                            }
                        }

                        // Ghi nhận số tiền THỰC TẾ đã chuyển, không phải số admin đề nghị.
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
