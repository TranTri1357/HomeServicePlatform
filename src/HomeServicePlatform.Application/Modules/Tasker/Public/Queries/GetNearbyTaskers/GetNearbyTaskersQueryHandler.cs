using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Tasker.Dtos;
using MediatR;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Public.Queries.GetNearbyTaskers
{
    public class GetNearbyTaskersQueryHandler : IRequestHandler<GetNearbyTaskersQuery, ApiResponse<List<NearbyTaskerDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetNearbyTaskersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<NearbyTaskerDto>>> Handle(GetNearbyTaskersQuery request, CancellationToken ct)
        {
            // Khởi tạo Factory tọa độ chuẩn GPS (SRID 4326)
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var customerLocation = geometryFactory.CreatePoint(new Coordinate(request.CustomerLng, request.CustomerLat));

            // độ kinh tuyến/vĩ tuyến xấp xỉ 111.12 km tại xích đạo
            // Đổi RadiusKm sang đơn vị Độ (Degrees) để PostGIS tính toán nội bộ
            double radiusInDegrees = request.RadiusKm / 111.12;

            var nearbyTaskers = await _context.TaskerProfiles
                .AsNoTracking()
                .Where(t =>
                    !t.IsDeleted &&
                    t.CurrentGeom != null &&
                    t.Status == 1 && // chỉ thợ đang nhận việc; loại thợ bị khóa(2)/tạm nghỉ(3)/từ chối(4)/chờ duyệt(0)
                    t.TaskerServices.Any(ts => ts.ServiceId == request.ServiceId) &&
                    t.CurrentGeom.Distance(customerLocation) <= radiusInDegrees)
                .Select(t => new NearbyTaskerDto
                {
                    TaskerId = t.TaskerProfileId,
                    FullName = t.User.FullName,
                    Latitude = t.CurrentGeom!.Y,
                    Longitude = t.CurrentGeom.X,
                    Status = t.Status,
                    RatingAvg = t.RatingAvg,

                    // Tính khoảng cách trả về cho FE hiển thị (nhân lại với 111.12 để ra Km)
                    DistanceKm = Math.Round(t.CurrentGeom.Distance(customerLocation) * 111.12, 1)
                })
                .ToListAsync(ct);

            return ApiResponse<List<NearbyTaskerDto>>.Success(nearbyTaskers, "Tìm thợ xung quanh thành công.");
        }
    }
}
