using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerJobs
{
    public class GetTaskerJobsQueryHandler : IRequestHandler<GetTaskerJobsQuery, ApiResponse<List<TaskerJobDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerJobsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<TaskerJobDto>>> Handle(GetTaskerJobsQuery request, CancellationToken cancellationToken)
        {
            // 1. Khởi tạo Query gốc kết nối trực tiếp trên các Entity thô dưới Database
            var sourceQuery = from item in _context.BookingItems
                              where item.TaskerId == request.TaskerId

                              join b in _context.Bookings on item.BookingId equals b.BookingId
                              join cust in _context.Users on b.CustomerId equals cust.UserId
                              join s in _context.Services on item.ServiceId equals s.ServiceId

                              join addr in _context.BookingAddresses on b.BookingId equals addr.BookingId into addrGroup
                              from subAddr in addrGroup.DefaultIfEmpty()
                              select new { item, b, cust, s, subAddr };

            // 1b. 🔒 LUỒNG CÁCH 2: Chỉ hiện cho thợ những đơn ĐÃ CHỐT — tức đã thanh toán thành công
            //     (Status==1) HOẶC đơn tiền mặt trả-khi-hoàn-thành (Status==0 & Method==Cash(2)).
            //     Đơn mới "giữ chỗ" chưa qua thanh toán (không có payment) không được lộ cho thợ.
            sourceQuery = sourceQuery.Where(q =>
                _context.Payments.Any(p => p.BookingId == q.b.BookingId
                                           && (p.Status == (short)PaymentStatus.Paid
                                               || (p.Status == (short)PaymentStatus.Pending && p.Method == (short)PaymentMethod.Cash))));

            // 2. 🟢 CHỌN LỌC HOẶC KHÔNG LỌC:
            // Nếu có truyền status -> Thêm điều kiện lọc. Nếu để trống -> Bỏ qua và lấy TẤT CẢ.
            if (request.Status.HasValue)
            {
                // Lọc theo trạng thái ĐƠN TỔNG (authoritative), khớp với hiển thị.
                sourceQuery = sourceQuery.Where(q => (short)q.b.Status == request.Status.Value);
            }

            // 3. Sắp xếp theo thứ tự thời gian công việc gần nhất lên đầu
            sourceQuery = sourceQuery.OrderBy(q => q.item.StartAt);

            // 4. Cuối cùng mới Projection nhào nặn cấu trúc ra DTO để trả về cho Client
            var result = await sourceQuery
                .Select(q => new TaskerJobDto(
                    q.item.BookingItemId,
                    q.b.BookingId,
                    q.s.Name,
                    q.cust.FullName,
                    q.cust.Phone,
                    q.item.StartAt,
                    q.item.EndAt,
                    q.subAddr != null ? $"{q.subAddr.AddressLine}, {q.subAddr.WardCode}" : "Chưa cập nhật địa chỉ",
                    q.item.TotalPrice,
                    (short)q.b.Status // Trạng thái đơn tổng (nguồn chuẩn), tránh lệch với item
                ))
                .ToListAsync(cancellationToken);

            return ApiResponse<List<TaskerJobDto>>.Success(result, "Lấy danh sách công việc của thợ thành công.");
        }
    }
}
