using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.UpdateBookingStatus
{
    public class UpdateBookingStatusCommandHandler : IRequestHandler<UpdateBookingStatusCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public UpdateBookingStatusCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(UpdateBookingStatusCommand request, CancellationToken ct)
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
                        // 1. Tìm đơn hàng tổng
                        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);
                        if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

                        // 2. Kích hoạt bẫy chặn bất đồng bộ native cho Booking tổng
                        efContext.Entry(booking).Property(b => b.RowVersion).OriginalValue = request.CurrentRowVersion;

                        short oldStatus = (short)booking.Status;
                        if (oldStatus == request.NewStatus)
                        {
                            return ApiResponse<bool>.Success(true, "Đơn hàng đã mang trạng thái này từ trước.");
                        }

                        // 3. Thực hiện thay đổi trạng thái đơn hàng tổng
                        booking.Status = (BookingStatus)request.NewStatus;
                        booking.UpdatedAt = now;
                        booking.RowVersion += 1;

                        // 4. 🟢 ĐỒNG BỘ DOANH NGHIỆP: Cập nhật toàn bộ các ca làm việc chi tiết (BookingItems)
                        var bookingItems = await _context.BookingItems
                            .Where(item => item.BookingId == booking.BookingId)
                            .ToListAsync(ct);

                        foreach (var item in bookingItems)
                        {
                            // Ép trạng thái của item đi theo trạng thái mới của đơn hàng tổng
                            item.Status = request.NewStatus;
                            item.UpdatedAt = now;

                            // Nếu bảng booking_items của bạn có row_version, hãy bảo vệ nó luôn:
                            item.RowVersion += 1;
                        }

                        // 5. Tự động ghi vết sự kiện biến đổi trạng thái vào lịch sử đơn hàng
                        var history = new BookingHistory
                        {
                            BookingId = booking.BookingId,
                            OldStatus = oldStatus,
                            NewStatus = request.NewStatus,
                            ChangedBy = request.ChangedBy,
                            CreatedAt = now
                        };
                        _context.BookingHistories.Add(history);

                        // 6. Lưu gộp dữ liệu xuống DB (EF Core chạy chung trong 1 Transaction an toàn)
                        await _context.SaveChangesAsync(ct);
                        await transaction.CommitAsync(ct);

                        return ApiResponse<bool>.Success(true, "Cập nhật trạng thái đơn đặt lịch thành công.");
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        await transaction.RollbackAsync(ct);
                        throw new BadRequestException("Dữ liệu đơn đặt lịch đã bị thay đổi ở một phiên làm việc khác. Vui lòng tải lại trang.");
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync(ct);
                        throw;
                    }
                });
            }
            throw new BadRequestException("Hệ thống lỗi không hỗ trợ bảo mật giao dịch.");
        }
    }
}
