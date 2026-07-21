using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.DeleteAddress
{
    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteAddressCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(DeleteAddressCommand request, CancellationToken ct)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == request.CustomerId, ct);

            if (address == null)
                throw new NotFoundException("Không tìm thấy địa chỉ cần xóa.");

            bool wasDefault = address.IsDefault == true;
            _context.Addresses.Remove(address);

            if (wasDefault)
            {
                var next = await _context.Addresses
                    .Where(a => a.UserId == request.CustomerId && a.AddressId != request.AddressId)
                    .OrderByDescending(a => a.AddressId)
                    .FirstOrDefaultAsync(ct);
                if (next != null) next.IsDefault = true;
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Xóa địa chỉ thành công.");
        }
    }
}
