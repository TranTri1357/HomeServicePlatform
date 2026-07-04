using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Operations.Tasker.Dtos;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Operations.Tasker.Queries.GetMyNotifications
{
    public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, ApiResponse<PagedResult<NotificationDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetMyNotificationsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<PagedResult<NotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken ct)
        {
            var query = _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == request.UserId)
                .OrderByDescending(n => n.CreatedAt); 

            var totalCount = await query.CountAsync(ct);

            var items = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    Type = n.Type,
                    Payload = n.Payload,
                    IsRead = n.Status == 1,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync(ct);

            var pagedResult = new PagedResult<NotificationDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = request.PageNumber,
                PageSize = request.PageSize
            };

            return ApiResponse<PagedResult<NotificationDto>>.Success(pagedResult, "Lấy danh sách thông báo thành công.");
        }
    }
}
