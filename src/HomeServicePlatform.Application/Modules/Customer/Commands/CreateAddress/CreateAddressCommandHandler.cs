using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Customer.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace HomeServicePlatform.Application.Modules.Customer.Commands.CreateAddress
{
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;
        private readonly GeometryFactory _geometryFactory;

        public CreateAddressCommandHandler(IApplicationDbContext context)
        {
            _context = context;
            _geometryFactory = new GeometryFactory(new PrecisionModel(), 4326); // WGS84 (PostGIS)
        }

        public async Task<ApiResponse<long>> Handle(CreateAddressCommand request, CancellationToken ct)
        {
            var existing = await _context.Addresses
                .Where(a => a.UserId == request.CustomerId)
                .ToListAsync(ct);

            // Địa chỉ đầu tiên luôn là mặc định; hoặc khi khách chủ động chọn mặc định.
            bool makeDefault = request.IsDefault || existing.Count == 0;
            if (makeDefault)
            {
                foreach (var a in existing.Where(a => a.IsDefault == true))
                    a.IsDefault = false;
            }

            Point? geom = null;
            if (request.Latitude.HasValue && request.Longitude.HasValue)
                geom = _geometryFactory.CreatePoint(new Coordinate(request.Longitude.Value, request.Latitude.Value));

            var address = new Address
            {
                UserId = request.CustomerId,
                ProvinceCode = request.ProvinceCode,
                DistrictCode = request.DistrictCode,
                WardCode = request.WardCode,
                AddressLine = request.AddressLine.Trim(),
                Geom = geom,
                IsDefault = makeDefault
            };

            _context.Addresses.Add(address);

            // Thợ: địa chỉ mới trở thành mặc định → đồng bộ vị trí trên bản đồ.
            if (makeDefault)
                await TaskerLocationSync.SyncFromDefaultAsync(_context, request.CustomerId, geom, ct);

            await _context.SaveChangesAsync(ct);

            return ApiResponse<long>.Success(address.AddressId, "Thêm địa chỉ thành công.");
        }
    }
}
