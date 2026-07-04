using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ProcessPaymentCallback
{
    public class ProcessPaymentCallbackCommandHandler : IRequestHandler<ProcessPaymentCallbackCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public ProcessPaymentCallbackCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(ProcessPaymentCallbackCommand request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            if (_context is DbContext efContext)
            {
                var strategy = efContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await efContext.Database.BeginTransactionAsync(ct);
                    try
                    {
                        // 1. Tìm bản ghi thanh toán theo ID (Đã được Controller gán an toàn từ URL)
                        var payment = await _context.Payments
                            .FirstOrDefaultAsync(x => x.PaymentId == request.PaymentId, ct);

                        if (payment == null)
                        {
                            throw new NotFoundException($"Không tìm thấy giao dịch thanh toán #{request.PaymentId}");
                        }

                        // 2. Kích hoạt cơ chế Optimistic Concurrency native của EF Core
                        efContext.Entry(payment).Property(x => x.RowVersion).OriginalValue = request.CurrentRowVersion;

                        // 3. Vệ binh trạng thái: Đã kết toán rồi thì không cho sửa đổi nữa
                        if (payment.Status == 1 || payment.Status == 2)
                        {
                            return ApiResponse<bool>.Success(true, "Giao dịch này đã được hệ thống định đoạt trạng thái từ trước.");
                        }

                        // 4. Cập nhật dữ liệu
                        payment.Status = request.NewStatus;
                        payment.TransactionCode = request.TransactionCode.Trim();
                        payment.UpdatedAt = now;

                        if (request.NewStatus == 1) // Nếu thanh toán thành công
                        {
                            payment.PaidAt = now;

                            // Tìm đơn hàng tương ứng đổi trạng thái đồng bộ
                            var booking = await _context.Bookings
                                .FirstOrDefaultAsync(b => b.BookingId == payment.BookingId, ct);

                            if (booking != null)
                            {
                                // Giả định trạng thái: Đơn đã thanh toán, sẵn sàng triển khai công việc
                                booking.Status = BookingStatus.Completed;
                                booking.UpdatedAt = now;
                            }
                        }

                        // 5. Đồng bộ lưu vết gộp trong 1 Database Transaction duy nhất
                        await _context.SaveChangesAsync(ct);
                        await transaction.CommitAsync(ct);

                        return ApiResponse<bool>.Success(true, "Xử lý kết quả thanh toán và cập nhật đơn đặt lịch thành công.");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        await transaction.RollbackAsync(ct);
                        throw new BadRequestException("Giao dịch đang được xử lý song song ở một tiến trình khác. Vui lòng thử lại.");
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync(ct);
                        throw;
                    }
                });
            }

            throw new BadRequestException("Hệ thống không hỗ trợ cơ chế bảo mật giao dịch gộp.");
        }
    }
}
