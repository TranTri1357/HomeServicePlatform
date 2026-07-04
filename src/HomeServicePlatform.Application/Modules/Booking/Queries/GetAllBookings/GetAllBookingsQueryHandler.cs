//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using HomeServicePlatform.Application.Common.Interfaces;
//using HomeServicePlatform.Application.Common.Pagination;
//using HomeServicePlatform.Application.Common.Responses;
//using MediatR;

//namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetAllBookings
//{
//    public class GetAllBookingsQueryHandler : IRequestHandler<GetAllBookingsQuery, ApiResponse<PagedResult<BookingLookupDto>>>
//    {
//        private readonly IApplicationDbContext _context;

//        public GetAllBookingsQueryHandler(IApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<ApiResponse<PagedResult<BookingLookupDto>>> Handle(GetAllBookingsQuery request, CancellationToken cancellationToken)
//        {
//            // 1. Khởi tạo LINQ Query nối các bảng qua cơ chế Explicit Join
//            var query = from b in _context.Bookings
//                            // 🛡️ BẢO VỆ CHUẨN DOANH NGHIỆP: Tận dụng cột và Index IsDeleted vừa tạo để lọc bỏ dòng đã xóa mềm
//                        where !b.IsDeleted

//                        join cust in _context.Users on b.CustomerId equals cust.UserId
//                        join addr in _context.BookingAddresses on b.BookingId equals addr.BookingId into addrGroup
//                        from subAddr in addrGroup.DefaultIfEmpty()
//                        select new { b, cust, subAddr };

//            // 2. Tích hợp bộ lọc tìm kiếm động (Dynamic Filters)
//            if (request.Status.HasValue)
//            {
//                query = query.Where(x => x.b.Status == request.Status.Value);
//            }

//            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
//            {
//                string search = request.SearchTerm.Trim().ToLower();
//                // Tìm kiếm thông minh theo cả Tên hoặc Số điện thoại khách hàng
//                query = query.Where(x => x.cust.FullName.ToLower().Contains(search)
//                                      || x.cust.Phone.Contains(search));
//            }

//            // 3. Thực thi đếm tổng số dòng thỏa mãn (Postgres quét qua Index cực nhanh)
//            int totalCount = await query.CountAsync(cancellationToken);

//            // 4. Phân trang dữ liệu đầu ra và Project thẳng về Flat DTO (Tự động kích hoạt cơ chế AsNoTracking ngầm)
//            var items = await query
//                .OrderByDescending(x => x.b.CreatedAt) // Ưu tiên các đơn đặt lịch mới nhất lên đầu danh sách
//                .Skip((request.PageIndex - 1) * request.PageSize)
//                .Take(request.PageSize)
//                .Select(x => new BookingLookupDto(
//                    x.b.BookingId,
//                    x.cust.FullName,
//                    x.cust.Phone,
//                    x.b.FinalAmount,
//                    (short)x.b.Status,
//                    x.b.CreatedAt,
//                    x.subAddr != null ? $"{x.subAddr.AddressLine}, {x.subAddr.WardCode}" : "Chưa cập nhật địa chỉ"
//                ))
//                .ToListAsync(cancellationToken);

//            // 5. Nạp toàn bộ dữ liệu vào class PagedResult định nghĩa sẵn của hệ thống bạn
//            var pagedResult = new PagedResult<BookingLookupDto>
//            {
//                Items = items,
//                TotalCount = totalCount,
//                PageIndex = request.PageIndex,
//                PageSize = request.PageSize
//            };

//            return ApiResponse<PagedResult<BookingLookupDto>>.Success(pagedResult, "Tải danh sách đơn hàng phân trang thành công.");
//        }
//    }
//}
