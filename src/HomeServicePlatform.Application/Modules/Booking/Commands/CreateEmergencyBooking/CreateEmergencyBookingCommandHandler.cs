using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Booking.Emergency;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking
{
    public class CreateEmergencyBookingCommandHandler
        : IRequestHandler<CreateEmergencyBookingCommand, ApiResponse<CreateEmergencyBookingResponse>>
    {
        // Vòng quét đầu tiên và thời gian thợ có để bấm nhận. Việc NỚI bán kính (10km, 15km) do
        // frontend điều phối qua re-broadcast khi hết vòng mà chưa ai nhận.
        private const double InitialRadiusKm = 5.0;
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
            if (string.IsNullOrWhiteSpace(request.AddressLine))
                throw new BadRequestException("Vui lòng nhập địa chỉ để thợ đến.");

            // 2. Dịch vụ phải tồn tại và đang hoạt động.
            var service = await _context.Services
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ServiceId == request.ServiceId && s.IsActive && !s.IsDeleted, ct);
            if (service == null)
                throw new NotFoundException("Không tìm thấy dịch vụ hoặc dịch vụ đã ngừng.");

            // 3. Dựng đơn khẩn cấp TREO MỞ: chưa gán thợ (TaskerId = null → KHÔNG giữ chỗ lịch của ai,
            //    khớp EXCLUDE constraint chỉ áp dụng khi tasker_id IS NOT NULL), giá = 0 (chốt khi thợ nhận),
            //    cửa sổ phản hồi 30s. Thợ nào bấm nhận trước sẽ được gán + chốt giá ở AcceptBooking.
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
                SubtotalAmount = 0m,
                DiscountAmount = 0m,
                FinalAmount = 0m
            };
            booking.InitializeBooking(request.CustomerId);

            booking.BookingItems.Add(new BookingItem
            {
                ServiceId = request.ServiceId,
                TaskerId = null, // chưa gán — treo mở cho mọi thợ trong vòng
                StartAt = startAtUtc,
                EndAt = endAtUtc,
                Quantity = 1,
                DurationMinutes = (int)(endAtUtc - startAtUtc).TotalMinutes,
                UnitPrice = 0m,
                TotalPrice = 0m,
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

            // 4. Lưu đơn (không cần advisory lock/buffer ở bước tạo vì chưa biết thợ — sẽ kiểm tra khi thợ nhận).
            await _bookingRepository.SaveAggregateAsync(booking);

            // 5. Quét thợ đủ điều kiện trong vòng đầu (5km) để controller bắn SignalR. Rỗng cũng không sao:
            //    frontend sẽ tự re-broadcast nới bán kính.
            var taskers = await EmergencyTaskerFinder.FindEligibleAsync(
                _context, request.ServiceId, request.Latitude, request.Longitude, InitialRadiusKm, ct);

            var response = new CreateEmergencyBookingResponse(
                booking.BookingId,
                service.Name,
                request.AddressLine,
                request.Latitude,
                request.Longitude,
                (int)ResponseWindow.TotalSeconds,
                InitialRadiusKm,
                taskers);

            return ApiResponse<CreateEmergencyBookingResponse>.Success(response, "Đã tạo yêu cầu khẩn cấp, đang tìm thợ.");
        }
    }
}
