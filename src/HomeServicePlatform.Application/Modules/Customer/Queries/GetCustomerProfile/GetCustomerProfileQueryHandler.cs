using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using HomeServicePlatform.Domain.Modules.Bookings.Enums;

namespace HomeServicePlatform.Application.Modules.Customer.Queries.GetCustomerProfile
{
    public class GetCustomerProfileQueryHandler : IRequestHandler<GetCustomerProfileQuery, ApiResponse<CustomerProfileDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerProfileQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<CustomerProfileDto>> Handle(GetCustomerProfileQuery request, CancellationToken cancellationToken)
        {
            // 1. Tìm thông tin User gốc của khách hàng
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.CustomerId && !u.IsDeleted, cancellationToken);

            if (user == null)
            {
                throw new NotFoundException($"Không tìm thấy hồ sơ khách hàng số #{request.CustomerId}");
            }

            // 2. Đếm tổng số đơn đã đặt dưới Database (Quét qua index ix_bookings_customer_id)
            var totalBookings = await _context.Bookings
                .CountAsync(b => b.CustomerId == request.CustomerId, cancellationToken);

            // 3. Đếm số đơn đã hoàn thành (Ví dụ trạng thái hoàn thành là 2, hãy chỉnh lại theo Business của bạn)
            var completedBookings = await _context.Bookings
                .CountAsync(b => b.CustomerId == request.CustomerId && b.Status == BookingStatus.Completed, cancellationToken);

            // 4. Lấy địa chỉ gần nhất mà khách hàng này từng sử dụng để đặt đơn
            var latestAddress = await (from b in _context.Bookings
                                       where b.CustomerId == request.CustomerId
                                       join addr in _context.BookingAddresses on b.BookingId equals addr.BookingId
                                       orderby b.CreatedAt descending
                                       select $"{addr.AddressLine}, {addr.WardCode}").FirstOrDefaultAsync(cancellationToken);

            // 5. Đóng gói kết quả trả về DTO
            var result = new CustomerProfileDto(
                user.UserId,
                user.FullName,
                user.Phone,
                user.Email,
                latestAddress ?? "Chưa cập nhật địa chỉ",
                totalBookings,
                completedBookings
            );

            return ApiResponse<CustomerProfileDto>.Success(result, "Lấy thông tin hồ sơ khách hàng thành công.");
        }
    }
}
