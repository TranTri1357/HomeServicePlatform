using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetCustomerAddresses
{
    public class GetCustomerAddressesQueryHandler : IRequestHandler<GetCustomerAddressesQuery, ApiResponse<List<AddressDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetCustomerAddressesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<AddressDto>>> Handle(GetCustomerAddressesQuery request, CancellationToken ct)
        {
            var addresses = await _context.Addresses
                .AsNoTracking()
                .Where(a => a.UserId == request.CustomerId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.AddressId)
                .Select(a => new AddressDto
                {
                    AddressId = a.AddressId,

                    ProvinceCode = a.ProvinceCode ?? string.Empty,
                    DistrictCode = a.DistrictCode ?? string.Empty,
                    WardCode = a.WardCode ?? string.Empty,

                    AddressLine = a.AddressLine,

                    IsDefault = a.IsDefault ?? false,

                    Latitude = a.Geom != null ? a.Geom.Y : 0,
                    Longitude = a.Geom != null ? a.Geom.X : 0
                })
                .ToListAsync(ct);

            return ApiResponse<List<AddressDto>>.Success(addresses, "Lấy danh sách địa chỉ thành công.");
        }
    }
}
