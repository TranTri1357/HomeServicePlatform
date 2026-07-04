//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HomeServicePlatform.Application.Common.Exceptions;
//using HomeServicePlatform.Application.Common.Interfaces;
//using HomeServicePlatform.Application.Common.Responses;
//using MediatR;

//namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetBookingDetail
//{
//    public class GetBookingDetailQueryHandler : IRequestHandler<GetBookingDetailQuery, ApiResponse<BookingDetailDto>>
//    {
//        private readonly IApplicationDbContext _context;

//        public GetBookingDetailQueryHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<ApiResponse<BookingDetailDto>> Handle(GetBookingDetailQuery request, CancellationToken cancellationToken)
//        {
//            // 🟢 TỐI ƯU DOANH NGHIỆP: Sử dụng một câu lệnh duy nhất để chiếu (Select) thẳng về DTO
//            // Cơ chế này giúp sinh ra câu lệnh SQL SELECT tường minh cột, cực kỳ nhẹ cho Database
//            var bookingDetail = await (
//                from b in _context.Bookings
//                where b.BookingId == request.BookingId && !b.IsDeleted

//                // Join lấy thông tin khách hàng đặt đơn (Inner Join vì đơn phải có khách)
//                join cust in _context.Users on b.CustomerId equals cust.UserId

//                // Left Join lấy thông tin địa chỉ đơn hàng (Phòng hờ trường hợp dữ liệu address bị lỗi)
//                join addr in _context.BookingAddresses on b.BookingId equals addr.BookingId into addrGroup
//                from subAddr in addrGroup.DefaultIfEmpty()

//                    // Nghiệp vụ: Lấy tên thợ thực hiện. 
//                    // Do thợ được phân bổ ở bảng BookingItem, ta lấy thợ của item đầu tiên (hoặc xử lý theo cấu hình hệ thống của bạn)
//                join item in _context.BookingItems on b.BookingId equals item.BookingId into itemGroup
//                from subItem in itemGroup.Take(1).DefaultIfEmpty()

//                    // Left Join tiếp từ item sang bảng Users để bốc tên của Thợ (Tasker)
//                join tasker in _context.Users on subItem.TaskerId equals tasker.UserId into taskerGroup
//                from subTasker in taskerGroup.DefaultIfEmpty()

//                select new BookingDetailDto(
//                    b.BookingId,
//                    cust.FullName,
//                    subTasker != null ? subTasker.FullName : "Hệ thống đang điều phối thợ...", // Xử lý hiển thị an toàn khi chưa có thợ
//                    (short)b.Status,
//                    b.SubtotalAmount,
//                    b.DiscountAmount,
//                    b.FinalAmount,
//                    b.CreatedAt.UtcDateTime, // Đưa về chuẩn hiển thị của C# DateTime
//                    subAddr != null ? $"{subAddr.AddressLine}, {subAddr.WardCode}" : "Chưa cập nhật địa chỉ"
//                )
//            ).FirstOrDefaultAsync(cancellationToken);

//            // Bẫy lỗi bảo vệ hệ thống nếu ID đơn không tồn tại hoặc đã bị xóa mềm trước đó
//            if (bookingDetail == null)
//            {
//                throw new NotFoundException($"Không tìm thấy dữ liệu chi tiết cho đơn hàng số #{request.BookingId}");
//            }

//            return ApiResponse<BookingDetailDto>.Success(bookingDetail, "Tải dữ liệu đơn hàng thành công.");
//        }
//    }
//}
