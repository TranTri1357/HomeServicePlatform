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
                        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);
                        if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

                        efContext.Entry(booking).Property(b => b.RowVersion).OriginalValue = request.CurrentRowVersion;

                        short oldStatus = (short)booking.Status;
                        if (oldStatus == request.NewStatus)
                        {
                            return ApiResponse<bool>.Success(true, "Đơn hàng đã mang trạng thái này từ trước.");
                        }

                        booking.Status = (BookingStatus)request.NewStatus;
                        booking.UpdatedAt = now;
                        booking.RowVersion += 1;

                        var bookingItems = await _context.BookingItems
                            .Where(item => item.BookingId == booking.BookingId)
                            .ToListAsync(ct);

                        foreach (var item in bookingItems)
                        {
                            item.Status = request.NewStatus;
                            item.UpdatedAt = now;

                            item.RowVersion += 1;
                        }

                        var history = new BookingHistory
                        {
                            BookingId = booking.BookingId,
                            OldStatus = oldStatus,
                            NewStatus = request.NewStatus,
                            ChangedBy = request.ChangedBy,
                            CreatedAt = now
                        };
                        _context.BookingHistories.Add(history);

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
