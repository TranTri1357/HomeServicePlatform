using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Modules.Tasker.Admin.Dtos;

namespace HomeServicePlatform.Application.Modules.Tasker.Admin.Queries.GetTaskerDetail
{
    public class GetTaskerDetailQueryHandler : IRequestHandler<GetTaskerDetailQuery, ApiResponse<TaskerDetailDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetTaskerDetailQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<TaskerDetailDto>> Handle(GetTaskerDetailQuery request, CancellationToken ct)
        {
            var taskerCore = await _context.TaskerProfiles
                .AsNoTracking()
                .Where(t => t.TaskerProfileId == request.TaskerId && !t.IsDeleted)
                .Select(t => new {
                    t.TaskerProfileId,
                    t.User.FullName,
                    t.User.Email,
                    t.User.Phone,
                    t.Bio,
                    t.ExperienceYears,
                    t.IsVerified,
                    t.VerifiedAt,
                    t.RatingAvg,
                    t.TotalReviews,
                    TaskerStatus = t.Status,
                    UserStatus = t.User.Status,
                    t.User.CreatedAt
                })
                .FirstOrDefaultAsync(ct);

            if (taskerCore == null)
                throw new NotFoundException($"Không tìm thấy hồ sơ thợ với ID {request.TaskerId}.");

            var skills = await _context.TaskerServices
                .AsNoTracking()
                .Where(ts => ts.TaskerId == request.TaskerId)
                .Select(ts => ts.Service.Name)
                .ToListAsync(ct);

            var taskerDetail = new TaskerDetailDto(
                taskerCore.TaskerProfileId,
                taskerCore.FullName,
                taskerCore.Email,
                taskerCore.Phone,
                taskerCore.Bio,
                taskerCore.ExperienceYears,
                taskerCore.IsVerified,
                taskerCore.VerifiedAt,
                taskerCore.RatingAvg,
                taskerCore.TotalReviews,
                taskerCore.TaskerStatus,
                taskerCore.UserStatus,
                taskerCore.CreatedAt,
                skills
            );

            return ApiResponse<TaskerDetailDto>.Success(taskerDetail, "Lấy chi tiết hồ sơ thợ thành công.");
        }
    }
}
