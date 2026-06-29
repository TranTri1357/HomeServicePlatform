using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;

using MediatR;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ApiResponse<CreateBookingResponse>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork; // 🟢 Tích hợp UnitOfWork quản lý Transaction gộp
        private readonly GeometryFactory _geometryFactory;

        public CreateBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326); // Chuẩn WGS84 cho PostGIS
        }

        public async Task<ApiResponse<CreateBookingResponse>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra tính hợp lệ của danh sách hạng mục đầu vào
            if (request.BookingItems == null || !request.BookingItems.Any())
            {
                throw new BadRequestException("Đơn đặt lịch bắt buộc phải có ít nhất một hạng mục dịch vụ.");
            }

            // 2. Khởi tạo đối tượng Root: Booking (Chưa gán tổng tiền)
            var booking = new Domain.Modules.Bookings.Entities.Booking
            {
                CustomerId = request.CustomerId,
                Note = request.Note,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                RowVersion = 1
            };

            // 🔥 KÍCH HOẠT NGHIỆP VỤ LÕI: Tự động đưa trạng thái về Pending và nạp lịch sử tuyến đầu
            booking.InitializeBooking(request.CustomerId);

            decimal totalSubtotalAmount = 0m;

            // 3. Duyệt mảng tuần tự để bóc tách và đóng gói từng dịch vụ con
            foreach (var item in request.BookingItems)
            {
                // Chuyển đổi mốc thời gian về chuẩn UTC để tránh lỗi lệch múi giờ hệ thống
                DateTime startAtUtc = item.StartAt.ToUniversalTime();
                DateTime endAtUtc = item.EndAt.ToUniversalTime();
                int durationMinutes = (int)(endAtUtc - startAtUtc).TotalMinutes;

                // Bẫy lỗi nghiệp vụ khoảng cách thời gian của TỪNG hạng mục
                if (durationMinutes <= 0)
                {
                    throw new BadRequestException($"Dịch vụ mã #{item.ServiceId} có thời gian kết thúc bắt buộc phải lớn hơn thời gian bắt đầu.");
                }

                decimal itemTotalPrice = item.UnitPrice * item.Quantity;
                totalSubtotalAmount += itemTotalPrice;

                booking.BookingItems.Add(new BookingItem
                {
                    ServiceId = item.ServiceId,
                    TaskerId = item.TaskerId, // Có thể null nếu hệ thống tự phân phối sau
                    StartAt = startAtUtc,
                    EndAt = endAtUtc,
                    Quantity = item.Quantity,
                    DurationMinutes = durationMinutes,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = itemTotalPrice,
                    Status = (short)BookingStatus.Pending, // Ép kiểu Enum sang short khớp cột smallint trong DB
                    RowVersion = 1
                });
            }

            // 4. Tính toán tài chính gộp cho toàn bộ khối Aggregate sau khi duyệt xong mảng con
            decimal discount = request.DiscountAmount ?? 0m;
            booking.SubtotalAmount = totalSubtotalAmount;
            booking.DiscountAmount = discount;
            booking.FinalAmount = Math.Max(0m, totalSubtotalAmount - discount);

            // 5. Đóng gói thông tin vị trí bản đồ địa lý (PostGIS Spatial Data)
            Point? locationGeom = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
            {
                locationGeom = _geometryFactory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));
            }

            booking.BookingAddress = new BookingAddress
            {
                FullName = request.FullName,
                Phone = request.Phone,
                ProvinceCode = request.ProvinceCode,
                DistrictCode = request.DistrictCode,
                WardCode = request.WardCode,
                AddressLine = request.AddressLine,
                Geom = locationGeom
            };

            // 6. Đưa Aggregate Root vào hàng chờ của Repository (Chưa thực thi xuống DB)
            await _bookingRepository.SaveAggregateAsync(booking);

            // 7. Chốt hạ: UnitOfWork ra lệnh kích hoạt Transaction lưu đồng thời 4 bảng 
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 8. Đóng gói dữ liệu phản hồi tiêu chuẩn qua lớp gác cổng ApiResponse
            var responseData = new CreateBookingResponse(
                booking.BookingId,
                "Đặt lịch hệ thống thành công!",
                true,
                booking.FinalAmount
            );

            return ApiResponse<CreateBookingResponse>.Success(responseData, "Khởi tạo đơn đặt lịch thành công.");
        }
    }
}
