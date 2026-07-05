using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Tasker.Entities;
using MediatR;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Commands.CreateTaskerProfile
{
    public class CreateTaskerProfileCommandHandler : IRequestHandler<CreateTaskerProfileCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;

        public CreateTaskerProfileCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<bool>> Handle(CreateTaskerProfileCommand request, CancellationToken ct)
        {
            var existingProfile = await _context.TaskerProfiles
                            .FirstOrDefaultAsync(t => t.TaskerProfileId == request.UserId, ct);

            if (existingProfile != null)
                throw new Exception("Tài khoản này đã có hồ sơ thợ.");

            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var currentLocation = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));

            var newProfile = new TaskerProfile
            {
                TaskerProfileId = request.UserId,
                Bio = request.Bio,
                ExperienceYears = request.ExperienceYears,
                CurrentGeom = currentLocation,
                IsVerified = false,
                RatingAvg = 0,
                TotalReviews = 0,
                Status = 0,
                IsDeleted = false
            };

            _context.TaskerProfiles.Add(newProfile);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Tạo hồ sơ thành công. Vui lòng chờ hệ thống xác minh.");
        }
    }
}
