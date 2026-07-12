using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.SetDefaultAddress
{
    public class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public SetDefaultAddressCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(SetDefaultAddressCommand request, CancellationToken ct)
        {
            var addresses = await _context.Addresses
                .Where(a => a.UserId == request.CustomerId)
                .ToListAsync(ct);

            var target = addresses.FirstOrDefault(a => a.AddressId == request.AddressId);
            if (target == null)
                throw new NotFoundException("Không tìm thấy địa chỉ.");

            foreach (var a in addresses)
                a.IsDefault = a.AddressId == request.AddressId;

            // Thợ: đặt địa chỉ mặc định mới → đồng bộ vị trí trên bản đồ.
            await TaskerLocationSync.SyncFromDefaultAsync(_context, request.CustomerId, target.Geom, ct);

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Đã đặt địa chỉ mặc định.");
        }
    }
}
