using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Operations.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking
{
    public class CreateEmergencyBookingCommandHandler
        : IRequestHandler<CreateEmergencyBookingCommand, ApiResponse<CreateEmergencyBookingResponse>>
    {
        // Bán kính quét thợ khẩn cấp (đồng bộ với Use Case trong đề tài) và thời gian thợ có để bấm nhận.
        private const double MaxRadiusKm = 5.0;
        private const double DegreesPerKm = 111.12; // xấp xỉ tại xích đạo (đồng bộ GetNearbyTaskers)
        private static readonly TimeSpan ResponseWindow = TimeSpan.FromSeconds(30);

        private readonly IBookingRepository _bookingRepository;
        private readonly IApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public CreateEmergencyBookingCommandHandler(IBookingRepository bookingRepository, IApplicationDbContext context)
        {
            _bookingRepository = bookingRepository;
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326); // WGS84 (PostGIS)
        }

        public async Task<ApiResponse<CreateEmergencyBookingResponse>> Handle(CreateEmergencyBookingCommand request, CancellationToken ct)
        {
            // 1. Kiểm tra đầu vào cơ bản.
            if (request.UnitPrice <= 0)
                throw new BadRequestException("Giá dịch vụ khẩn cấp không hợp lệ.");
            if (string.IsNullOrWhiteSpace(request.AddressLine))
                throw new BadRequestException("Vui lòng nhập địa chỉ để thợ đến.");

            // 2. Dịch vụ phải tồn tại và đang hoạt động.
            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == request.ServiceId && s.IsActive && !s.IsDeleted, ct);
            if (service == null)
                throw new NotFoundException("Không tìm thấy dịch vụ hoặc dịch vụ đã ngừng.");

            // 3. Thợ phải đang rảnh (Status==1), còn hoạt động, có vị trí và cung cấp dịch vụ này.
            var tasker = await _context.TaskerProfiles
                .AsNoTracking()
                .Include(t => t.TaskerServices)
                .FirstOrDefaultAsync(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted, ct);
            if (tasker == null)
                throw new NotFoundException("Không tìm thấy thợ.");
            if (tasker.Status != 1)
                throw new BadRequestException("Thợ hiện không sẵn sàng nhận việc.");
            if (tasker.CurrentGeom == null)
                throw new BadRequestException("Thợ chưa cập nhật vị trí, không thể gọi khẩn cấp.");
            if (!tasker.TaskerServices.Any(ts => ts.ServiceId == request.ServiceId))
                throw new BadRequestException("Thợ này không cung cấp dịch vụ đã chọn.");

            // 4. Khoảng cách phải trong bán kính 5km.
            var customerPoint = _geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
            var distanceKm = Math.Round(tasker.CurrentGeom.Distance(customerPoint) * DegreesPerKm, 1);
            if (distanceKm > MaxRadiusKm)
                throw new BadRequestException($"Thợ đã ở ngoài bán kính {MaxRadiusKm:0}km (cách {distanceKm}km).");

            // 5. Dựng đơn khẩn cấp: bắt đầu "ngay bây giờ", cửa sổ phản hồi 30s.
            var nowUtc = DateTimeOffset.UtcNow;
            var startAtUtc = nowUtc;
            var endAtUtc = nowUtc.AddMinutes(service.DurationMinutes > 0 ? service.DurationMinutes : 60);

            var booking = new Domain.Modules.Bookings.Entities.Booking
            {
                CustomerId = request.CustomerId,
                Note = request.Note,
                CreatedAt = nowUtc,
                UpdatedAt = nowUtc,
                RowVersion = 1,
                IsEmergency = true,
                EmergencyExpiresAt = nowUtc.Add(ResponseWindow),
                SubtotalAmount = request.UnitPrice,
                DiscountAmount = 0m,
                FinalAmount = request.UnitPrice
            };
            booking.InitializeBooking(request.CustomerId);

            booking.BookingItems.Add(new BookingItem
            {
                ServiceId = request.ServiceId,
                TaskerId = request.TaskerId,
                StartAt = startAtUtc,
                EndAt = endAtUtc,
                Quantity = 1,
                DurationMinutes = (int)(endAtUtc - startAtUtc).TotalMinutes,
                UnitPrice = request.UnitPrice,
                TotalPrice = request.UnitPrice,
                Status = (short)BookingStatus.Pending,
                RowVersion = 1
            });

            var geom = _geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
            booking.BookingAddress = new BookingAddress
            {
                FullName = request.FullName,
                Phone = request.Phone,
                ProvinceCode = request.ProvinceCode,
                DistrictCode = request.DistrictCode,
                WardCode = request.WardCode,
                AddressLine = request.AddressLine,
                Geom = geom
            };

            // 6. Thông báo (bảng notifications) cho thợ — được lưu cùng transaction với booking.
            _context.Notifications.Add(NotificationBuilder.Build(
                request.TaskerId,
                NotificationType.EmergencyBooking,
                "Đơn khẩn cấp!",
                $"Có khách cần {service.Name} gấp, cách bạn {distanceKm}km. Phản hồi trong 30 giây."));

            // 7. Lưu Aggregate (booking + item + address + notification cùng 1 lần lưu).
            try
            {
                await _bookingRepository.SaveAggregateAsync(booking);
            }
            catch (DbUpdateException ex) when (IsExclusionViolation(ex))
            {
                throw new BadRequestException("Thợ vừa nhận một việc khác trùng giờ. Vui lòng chọn thợ khác.");
            }

            var response = new CreateEmergencyBookingResponse(
                booking.BookingId,
                request.TaskerId,
                service.Name,
                request.AddressLine,
                booking.FinalAmount,
                distanceKm,
                (int)ResponseWindow.TotalSeconds);

            return ApiResponse<CreateEmergencyBookingResponse>.Success(response, "Đã gửi yêu cầu khẩn cấp tới thợ.");
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
