using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Commissions.Queries.GetCommissionDetail
{
    public class GetCommissionDetailQueryHandler : IRequestHandler<GetCommissionDetailQuery, ApiResponse<CommissionDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetCommissionDetailQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<CommissionDto>> Handle(GetCommissionDetailQuery request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var data = await (from c in _context.Commissions
                              where c.CommissionId == request.CommissionId
                              join s in _context.Services on c.ServiceId equals s.ServiceId into sGroup
                              from subService in sGroup.DefaultIfEmpty()
                              join u in _context.Users on c.TaskerId equals u.UserId into uGroup
                              from subUser in uGroup.DefaultIfEmpty()
                              select new CommissionDto(
                                  c.CommissionId,
                                  c.ServiceId,
                                  subService != null ? subService.Name : "Áp dụng toàn sàn",
                                  c.TaskerId,
                                  subUser != null ? subUser.FullName : "Áp dụng tất cả thợ",
                                  c.CommissionRate,
                                  c.EffectiveFrom,
                                  c.EffectiveTo,
                                  c.EffectiveFrom <= now && (c.EffectiveTo == null || c.EffectiveTo > now)
                              )).FirstOrDefaultAsync(ct);

            if (data == null) throw new NotFoundException($"Không tìm thấy biểu phí hoa hồng số #{request.CommissionId}");

            return ApiResponse<CommissionDto>.Success(data, "Lấy chi tiết cấu hình hoa hồng thành công.");
        }
    }
}
