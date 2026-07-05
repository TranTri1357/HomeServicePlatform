using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Modules.Services.Admin.Dtos;

namespace HomeServicePlatform.Application.Modules.Services.Admin.Queries.GetServiceDetail
{
    public class GetServiceDetailQueryHandler : IRequestHandler<GetServiceDetailQuery, ApiResponse<ServiceDetailDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetServiceDetailQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<ServiceDetailDto>> Handle(GetServiceDetailQuery request, CancellationToken cancellationToken)
        {
            var service = await _context.Services
                .AsNoTracking()
                .Where(x => x.ServiceId == request.ServiceId && !x.IsDeleted)
                .Select(x => new ServiceDetailDto(
                    x.ServiceId,
                    x.CategoryId, 
                    x.Name,
                    x.Description,
                    x.DurationMinutes,
                    x.IsActive
                ))
                .FirstOrDefaultAsync(cancellationToken);

            if (service == null)
                throw new NotFoundException("Không tìm thấy dịch vụ.");

            return ApiResponse<ServiceDetailDto>.Success(service, "Lấy thông tin chi tiết thành công");
        }
    }
}
