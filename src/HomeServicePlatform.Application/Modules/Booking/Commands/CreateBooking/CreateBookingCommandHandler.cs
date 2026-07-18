using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Bookings.Entities;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;
using HomeServicePlatform.Domain.Modules.Bookings.Interface;
using HomeServicePlatform.Domain.Modules.Payments.Enum;

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateBooking
{
    public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, ApiResponse<CreateBookingResponse>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUnitOfWork _unitOfWork; // 🟢 Tích hợp UnitOfWork quản lý Transaction gộp
        private readonly IApplicationDbContext _context;
        private readonly BufferPolicyOptions _buffer;
        private readonly GeometryFactory _geometryFactory;

        public CreateBookingCommandHandler(IBookingRepository bookingRepository, IUnitOfWork unitOfWork,
            IApplicationDbContext context, IOptions<BufferPolicyOptions> buffer)
        {
            _bookingRepository = bookingRepository;
            _unitOfWork = unitOfWork;
            _context = context;
            _buffer = buffer.Value;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326); // Chuẩn WGS84 cho PostGIS
        }

        public async Task<ApiResponse<CreateBookingResponse>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // 1. Kiểm tra tính hợp lệ của danh sách hạng mục đầu vào
            if (request.BookingItems == null || !request.BookingItems.Any())
            {
                throw new BadRequestException("Đơn đặt lịch bắt buộc phải có ít nhất một hạng mục dịch vụ.");
            }

            var nowUtc = DateTimeOffset.UtcNow;

            // ♻️ Việc NHẢ CHỖ GIỮ HẾT HẠN (TTL) đã chuyển sang job nền
            //    ExpiredBookingCleanupService (chạy định kỳ), không còn quét nội tuyến ở đây
            //    để đường đặt đơn gọn nhẹ và slot vẫn được nhả đúng hạn kể cả khi không ai đặt mới.

            // 1c. 🛡️ CHỐNG GIẢ MẠO GIÁ: mỗi hạng mục BẮT BUỘC phải chọn 1 thợ cụ thể để
            //     server tra được đơn giá NIÊM YẾT thật. KHÔNG tin đơn giá client gửi lên.
            if (request.BookingItems.Any(i => !i.TaskerId.HasValue))
            {
                throw new BadRequestException("Mỗi hạng mục dịch vụ phải chọn một thợ cụ thể để xác định giá.");
            }

            // 1d. Nạp sẵn BẢNG GIÁ NIÊM YẾT còn hiệu lực cho các cặp (thợ, dịch vụ) trong đơn.
            //     Đây là NGUỒN GIÁ DUY NHẤT server tin — client gửi gì cũng bị bỏ qua.
            var taskerIds = request.BookingItems.Select(i => i.TaskerId!.Value).Distinct().ToList();
            var serviceIds = request.BookingItems.Select(i => i.ServiceId).Distinct().ToList();
            var priceRows = await _context.TaskerServicePrices
                .Where(p => taskerIds.Contains(p.TaskerId)
                            && serviceIds.Contains(p.ServiceId)
                            && p.EffectiveFrom <= nowUtc
                            && (p.EffectiveTo == null || p.EffectiveTo > nowUtc))
                .ToListAsync(cancellationToken);

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

                // 🛡️ Lấy ĐƠN GIÁ THẬT từ bảng niêm yết (mới nhất còn hiệu lực). Bỏ qua item.UnitPrice.
                var listedPrice = priceRows
                    .Where(p => p.TaskerId == item.TaskerId!.Value && p.ServiceId == item.ServiceId)
                    .OrderByDescending(p => p.EffectiveFrom)
                    .Select(p => (decimal?)p.Price)
                    .FirstOrDefault();

                if (listedPrice is null)
                {
                    throw new BadRequestException($"Thợ chưa niêm yết giá cho dịch vụ #{item.ServiceId}. Vui lòng chọn thợ hoặc dịch vụ khác.");
                }

                decimal serverUnitPrice = listedPrice.Value;
                decimal itemTotalPrice = serverUnitPrice * item.Quantity;
                totalSubtotalAmount += itemTotalPrice;

                booking.BookingItems.Add(new BookingItem
                {
                    ServiceId = item.ServiceId,
                    TaskerId = item.TaskerId, // Đã đảm bảo not-null ở bước 1c
                    StartAt = startAtUtc,
                    EndAt = endAtUtc,
                    Quantity = item.Quantity,
                    DurationMinutes = durationMinutes,
                    UnitPrice = serverUnitPrice, // 🛡️ Giá server, KHÔNG phải giá client gửi
                    TotalPrice = itemTotalPrice,
                    Status = (short)BookingStatus.Pending, // Ép kiểu Enum sang short khớp cột smallint trong DB
                    RowVersion = 1
                });
            }

            // 4. Tính toán tài chính gộp cho toàn bộ khối Aggregate sau khi duyệt xong mảng con.
            //    Giai đoạn này CHƯA có hệ thống mã khuyến mãi thật -> ép discount = 0 (không nhận
            //    DiscountAmount từ client để tránh khách tự đặt giảm giá về 0đ). Mở lại khi có coupon.
            decimal discount = 0m;
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

            // 6+7. Lưu Aggregate Root xuống DB rồi chốt — CHỐNG DOUBLE-BOOKING + ĐỆM DI CHUYỂN:
            //   • Mở transaction, giữ advisory lock theo TỪNG thợ (khóa theo thứ tự tăng dần để
            //     tránh deadlock) → tuần tự hóa các đơn đồng thời của cùng một thợ.
            //   • Kiểm tra buffer di chuyển tới đơn liền trước/sau (tầng ứng dụng, theo khoảng cách).
            //   • Lưu Aggregate (tham gia transaction hiện tại) rồi commit.
            //   🛡️ Nếu có đơn ĐÈ GIỜ thật (overlap), CSDL bật exclusion constraint gốc (SQLSTATE 23P01)
            //      -> dịch thành lỗi nghiệp vụ thân thiện. (Buffer di chuyển do TravelBufferGuard lo.)
            var destLat = request.Latitude;
            var destLng = request.Longitude;
            var lockTaskerIds = booking.BookingItems
                .Where(i => i.TaskerId.HasValue)
                .Select(i => i.TaskerId!.Value)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                foreach (var tid in lockTaskerIds)
                    await _context.AcquireTaskerScheduleLockAsync(tid, cancellationToken);

                foreach (var item in booking.BookingItems.Where(i => i.TaskerId.HasValue))
                    await TravelBufferGuard.EnsureTravelFeasibleAsync(
                        _context, _buffer, item.TaskerId!.Value, item.StartAt, item.EndAt, destLat, destLng, cancellationToken);

                await _bookingRepository.SaveAggregateAsync(booking);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (DbUpdateException ex) when (IsExclusionViolation(ex))
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw new BadRequestException(
                    "Rất tiếc, khung giờ bạn chọn của thợ vừa có người khác đặt trước. Vui lòng chọn khung giờ hoặc thợ khác.");
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }

            // 🔔 Lưu ý luồng Cách 2: KHÔNG bắn thông báo cho thợ tại đây. Đơn lúc này mới chỉ
            //    "giữ chỗ" (chưa thanh toán). Thông báo cho thợ được bắn ở bước Checkout thành công.

            // 8. Đóng gói dữ liệu phản hồi tiêu chuẩn qua lớp gác cổng ApiResponse
            var responseData = new CreateBookingResponse(
                booking.BookingId,
                "Đặt lịch hệ thống thành công!",
                true,
                booking.FinalAmount
            );

            return ApiResponse<CreateBookingResponse>.Success(responseData, "Khởi tạo đơn đặt lịch thành công.");
        }

        // Nhận diện lỗi vi phạm exclusion constraint (Postgres SQLSTATE 23P01) mà không cần
        // Application layer phụ thuộc trực tiếp vào Npgsql — đọc thuộc tính SqlState qua reflection.
        private static bool IsExclusionViolation(Exception ex)
        {
            for (var inner = ex.InnerException; inner != null; inner = inner.InnerException)
            {
                var sqlState = inner.GetType().GetProperty("SqlState")?.GetValue(inner) as string;
                if (sqlState == "23P01")
                    return true;
            }
            return false;
        }
    }
}
