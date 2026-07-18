using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.AcceptBooking
{
    public class AcceptBookingCommandHandler : IRequestHandler<AcceptBookingCommand, ApiResponse<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BufferPolicyOptions _buffer;

        public AcceptBookingCommandHandler(
            IBookingRepository bookingRepository,
            IApplicationDbContext context,
            IUnitOfWork unitOfWork,
            IOptions<BufferPolicyOptions> buffer)
        {
            _bookingRepository = bookingRepository;
            _context = context;
            _unitOfWork = unitOfWork;
            _buffer = buffer.Value;
        }

        public async Task<ApiResponse<bool>> Handle(AcceptBookingCommand request, CancellationToken cancellationToken)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId);
            if (booking == null) throw new NotFoundException($"Không tìm thấy đơn hàng #{request.BookingId}");

            // 🚨 Đơn khẩn cấp broadcast: "ai nhận trước thắng" — xử lý riêng để chống hai thợ giành cùng lúc.
            if (booking.IsEmergency)
                return await AcceptEmergencyAsync(booking, request, cancellationToken);

            // ── Luồng đặt lịch thường (giữ nguyên hành vi cũ) ──
            // 🔒 CHỐNG CƯỚP ĐƠN: đơn thường do khách chọn sẵn thợ; chỉ thợ ĐƯỢC GÁN mới được nhận.
            //    (Đơn khẩn cấp "ai nhận trước thắng" đã tách nhánh ở trên, không qua đây.)
            if (!booking.BookingItems.Any() || !booking.BookingItems.All(i => i.TaskerId == request.TaskerId))
                throw new ForbiddenException("Đơn này được chỉ định cho thợ khác, bạn không thể nhận.");

            try
            {
                booking.AcceptByTasker(request.TaskerId);

                // 🔔 Thông báo cho khách (cùng transaction với UpdateAggregateAsync).
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.BookingAccepted,
                    "Thợ đã nhận đơn",
                    $"Đơn BK{booking.BookingId} đã được thợ tiếp nhận."));

                await _bookingRepository.UpdateAggregateAsync(booking);
                return ApiResponse<bool>.Success(true, "Thợ đã xác nhận nhận lịch làm việc.");
            }
            catch (InvalidOperationException ex) { throw new BadRequestException(ex.Message); }
        }

        /// <summary>
        /// Thợ nhận một đơn khẩn cấp broadcast. Giữ advisory lock theo ĐƠN để tuần tự hóa: chỉ thợ vào
        /// trước thấy đơn còn Pending và thắng; thợ sau thấy đã Accepted → báo "đã có thợ khác nhận".
        /// Đồng thời gán thợ, chốt giá theo thợ, tạo Payment tiền mặt và kiểm tra đệm di chuyển
        /// (buffer) + exclusion constraint cho chính thợ nhận — tất cả trong CÙNG một transaction.
        /// </summary>
        private async Task<ApiResponse<bool>> AcceptEmergencyAsync(
            Domain.Modules.Bookings.Entities.Booking booking, AcceptBookingCommand request, CancellationToken ct)
        {
            var nowUtc = DateTimeOffset.UtcNow;
            if (booking.EmergencyExpiresAt.HasValue && nowUtc > booking.EmergencyExpiresAt.Value)
                throw new BadRequestException("Đơn khẩn cấp đã hết hạn, không thể nhận.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Giành đơn: khóa theo đơn rồi ĐỌC LẠI trạng thái mới nhất từ DB.
                await _context.AcquireBookingClaimLockAsync(booking.BookingId, ct);

                var currentStatus = await _context.Bookings.AsNoTracking()
                    .Where(b => b.BookingId == booking.BookingId)
                    .Select(b => (short)b.Status)
                    .FirstOrDefaultAsync(ct);
                if (currentStatus != (short)BookingStatus.Pending)
                    throw new BadRequestException("Rất tiếc, đơn khẩn cấp đã có thợ khác nhận.");

                // Chốt giá theo giá dịch vụ của chính thợ nhận.
                var serviceId = booking.BookingItems.Select(i => i.ServiceId).First();
                var price = await _context.TaskerServicePrices.AsNoTracking()
                    .Where(p => p.TaskerId == request.TaskerId && p.ServiceId == serviceId
                                && (p.EffectiveTo == null || p.EffectiveTo > nowUtc))
                    .Select(p => p.Price)
                    .FirstOrDefaultAsync(ct);
                if (price <= 0)
                    throw new BadRequestException("Bạn chưa cấu hình giá cho dịch vụ này nên không thể nhận đơn.");

                booking.SubtotalAmount = price;
                booking.FinalAmount = price;
                foreach (var item in booking.BookingItems)
                {
                    item.UnitPrice = price;
                    item.TotalPrice = price;
                }

                // Gán thợ + chuyển trạng thái (Pending → Accepted) qua state-machine.
                booking.AcceptByTasker(request.TaskerId);

                // 🔔 Thông báo cho khách.
                _context.Notifications.Add(NotificationBuilder.Build(
                    booking.CustomerId,
                    NotificationType.BookingAccepted,
                    "Thợ đã nhận đơn",
                    $"Đơn BK{booking.BookingId} đã được thợ tiếp nhận."));

                // 🚨 Đơn khẩn thanh toán tiền mặt sau: tạo Payment tiền mặt (Pending) khi thợ nhận.
                _context.Payments.Add(new Payment
                {
                    BookingId = booking.BookingId,
                    Amount = price,
                    Method = (short)PaymentMethod.Cash,
                    Status = (short)PaymentStatus.Pending,
                    CreatedAt = nowUtc,
                    RowVersion = 1
                });

                // Đệm di chuyển + chống double-booking cho CHÍNH thợ nhận (giờ mới biết thợ là ai).
                // Lấy tọa độ điểm hẹn riêng vì GetByIdAsync không nạp sẵn BookingAddress.
                await _context.AcquireTaskerScheduleLockAsync(request.TaskerId, ct);
                var item0 = booking.BookingItems.First();
                var dest = await _context.BookingAddresses.AsNoTracking()
                    .Where(a => a.BookingId == booking.BookingId && a.Geom != null)
                    .Select(a => new { Lat = a.Geom!.Y, Lng = a.Geom.X })
                    .FirstOrDefaultAsync(ct);
                await TravelBufferGuard.EnsureTravelFeasibleAsync(
                    _context, _buffer, request.TaskerId, item0.StartAt, item0.EndAt, dest?.Lat, dest?.Lng, ct);

                await _bookingRepository.UpdateAggregateAsync(booking);
                await _unitOfWork.CommitTransactionAsync();
                return ApiResponse<bool>.Success(true, "Đã nhận đơn khẩn cấp.");
            }
            catch (DbUpdateException ex) when (IsExclusionViolation(ex))
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new BadRequestException("Bạn đang có việc trùng giờ nên không thể nhận đơn này.");
            }
            catch (InvalidOperationException ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new BadRequestException(ex.Message);
            }
            catch
            {
                // Mọi lỗi khác (gồm BadRequestException từ các bước kiểm tra bên trên): rollback rồi ném tiếp.
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        // Nhận diện lỗi exclusion constraint (Postgres SQLSTATE 23P01) qua reflection.
        private static bool IsExclusionViolation(Exception ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner) as string;
                if (sqlState == "23P01") return true;
            }
            return false;
        }
    }
}
