using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Tasker.Queries.GetTaskerServices
{
    public class GetTaskerServicesQueryHandler : IRequestHandler<GetTaskerServicesQuery, ApiResponse<List<TaskerServiceDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetTaskerServicesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<TaskerServiceDto>>> Handle(GetTaskerServicesQuery request, CancellationToken cancellationToken)
        {
            // Kết nối các bảng: tasker_services -> services -> categories
            var query = from ts in _context.TaskerServices
                        where ts.TaskerId == request.TaskerProfileId

                        join s in _context.Services on ts.ServiceId equals s.ServiceId
                        join c in _context.Categories on s.CategoryId equals c.CategoryId

                        // 🟢 Join thêm bảng giá để lấy cột Price
                        join p in _context.TaskerServicePrices on
                            new { ts.TaskerId, ts.ServiceId } equals new { p.TaskerId, p.ServiceId } into priceGroup
                        from subPrice in priceGroup.Where(x => x.EffectiveTo == null).DefaultIfEmpty() // Lấy giá hiện tại

                        select new TaskerServiceDto(
                            ts.TaskerId, // Thay cho TaskerServiceId nếu bảng này là bảng trung gian không có Id tăng tự động
                            s.ServiceId,
                            s.Name,
                            c.Name,
                            subPrice != null ? subPrice.Price : 0, // 🟢 Lấy từ bảng giá phụ
                            s.DurationMinutes,
                            s.IsActive, // 🟢 Lấy trạng thái hoạt động trực tiếp từ bảng gốc Services (hoặc s.IsDeleted tùy logic)
                            s.ImageUrl
                        );

            // Thực thi truy vấn bất đồng bộ tối ưu hóa hiệu năng
            var result = await query.ToListAsync(cancellationToken);

            return ApiResponse<List<TaskerServiceDto>>.Success(result, "Lấy danh sách dịch vụ của thợ thành công.");
        }
    }
}
