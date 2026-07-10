using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.UpdateAddress
{
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public UpdateAddressCommandHandler(IApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);
        }

        public async Task<ApiResponse<bool>> Handle(UpdateAddressCommand request, CancellationToken ct)
        {
            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressId == request.AddressId && a.UserId == request.CustomerId, ct);

            if (address == null)
                throw new NotFoundException("Không tìm thấy địa chỉ cần cập nhật.");

            address.ProvinceCode = request.ProvinceCode;
            address.DistrictCode = request.DistrictCode;
            address.WardCode = request.WardCode;
            address.AddressLine = request.AddressLine.Trim();

            if (request.Latitude.HasValue && request.Longitude.HasValue)
                address.Geom = _geometryFactory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));

            // Nếu đặt làm mặc định thì gỡ mặc định ở các địa chỉ khác.
            if (request.IsDefault && address.IsDefault != true)
            {
                var others = await _context.Addresses
                    .Where(a => a.UserId == request.CustomerId && a.AddressId != request.AddressId && a.IsDefault == true)
                    .ToListAsync(ct);
                foreach (var a in others) a.IsDefault = false;
                address.IsDefault = true;
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật địa chỉ thành công.");
        }
    }
}
