using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetMyBookings
{
    public class GetMyBookingsQueryHandler : IRequestHandler<GetMyBookingsQuery, ApiResponse<List<MyBookingDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyBookingsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<MyBookingDto>>> Handle(GetMyBookingsQuery request, CancellationToken cancellationToken)
        {
            // 1. Khởi tạo Query kết hợp Explicit Join tối ưu hóa tốc độ quét Index
            var query = from b in _context.Bookings
                        where b.CustomerId == request.CustomerId

                        // Left Join sang bảng địa chỉ đơn hàng
                        join addr in _context.BookingAddresses on b.BookingId equals addr.BookingId into addrGroup
                        from subAddr in addrGroup.DefaultIfEmpty()

                        select new
                        {
                            Booking = b,
                            Address = subAddr,
                            // Gom dữ liệu từ bảng booking_items và map an toàn thông tin Thợ/Dịch vụ
                            FirstItem = _context.BookingItems
                                                .Where(i => i.BookingId == b.BookingId)
                                                .Select(i => new
                                                {
                                                    i.StartAt,
                                                    i.EndAt,
                                                    i.TaskerId,
                                                    ServiceName = i.Service.Name,
                                                    // Sử dụng toán tử kiểm tra Null an toàn tuyệt đối
                                                    TaskerName = i.TaskerProfile != null && i.TaskerProfile.User != null
                                                                 ? i.TaskerProfile.User.FullName
                                                                 : "Đang tìm thợ..."
                                                })
                                                .FirstOrDefault()
                        };

            // 2. Thực thi Projection trực tiếp xuống SQL Server/PostgreSQL bằng lệnh Async
            var result = await query
                .OrderByDescending(q => q.Booking.CreatedAt) // Đơn mới nhất lên đầu
                .Select(q => new MyBookingDto(
                    q.Booking.BookingId,
                    q.FirstItem != null ? q.FirstItem.ServiceName : "Dịch vụ hệ thống",
                    q.FirstItem != null ? q.FirstItem.TaskerId : null,
                    q.FirstItem != null ? q.FirstItem.TaskerName : "Đang tìm thợ...",
                    q.FirstItem != null ? q.FirstItem.StartAt : q.Booking.CreatedAt,
                    q.FirstItem != null ? q.FirstItem.EndAt : q.Booking.CreatedAt,
                    q.Address != null
                        ? $"{q.Address.AddressLine}, {q.Address.WardCode}" // Đã sửa biến chính xác sang 'q'
                        : "Chưa cập nhật địa chỉ",
                    q.Booking.FinalAmount,
                    (short)q.Booking.Status
                ))
                .ToListAsync(cancellationToken); // 🟢 Kích hoạt bất đồng bộ, giải phóng Thread hệ thống

            return ApiResponse<List<MyBookingDto>>.Success(result, "Lấy lịch sử đơn đặt lịch thành công.");
        }
    }
}