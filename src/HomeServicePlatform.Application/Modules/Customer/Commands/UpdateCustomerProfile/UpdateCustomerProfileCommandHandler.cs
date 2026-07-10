using System;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateCustomerProfile
{
    public class UpdateCustomerProfileCommandHandler
        : IRequestHandler<UpdateCustomerProfileCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateCustomerProfileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(UpdateCustomerProfileCommand request, CancellationToken ct)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == request.CustomerId && !u.IsDeleted, ct);

            if (user == null)
                throw new NotFoundException($"Không tìm thấy hồ sơ khách hàng số #{request.CustomerId}.");

            var newPhone = request.Phone.Trim();

            // Chặn trùng số điện thoại với tài khoản khác
            bool phoneTaken = await _context.Users
                .AnyAsync(u => u.Phone == newPhone && u.UserId != request.CustomerId && !u.IsDeleted, ct);
            if (phoneTaken)
                throw new BadRequestException("Số điện thoại này đã được sử dụng bởi tài khoản khác.");

            user.FullName = request.FullName.Trim();
            user.Phone = newPhone;
            user.UpdatedAt = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật hồ sơ thành công.");
        }
    }
}
