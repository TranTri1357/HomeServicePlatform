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
            var now = DateTimeOffset.UtcNow;

            // Kết nối các bảng: tasker_services -> services -> categories
            var query = from ts in _context.TaskerServices
                        where ts.TaskerId == request.TaskerProfileId

                        join s in _context.Services on ts.ServiceId equals s.ServiceId
                        join c in _context.Categories on s.CategoryId equals c.CategoryId

                        select new TaskerServiceDto(
                            ts.TaskerId, // Thay cho TaskerServiceId nếu bảng này là bảng trung gian không có Id tăng tự động
                            s.ServiceId,
                            s.Name,
                            c.Name,
                            // 💰 Giá đang hiệu lực — đồng bộ với TaskerPriceQuery.IsActiveAt.
                            //    ⚠️ Cố tình dùng TRUY VẤN CON thay vì group join + DefaultIfEmpty:
                            //    EF Core 8 KHÔNG dịch nổi `groupJoin.Where(...).OrderByDescending(...)
                            //    .DefaultIfEmpty()` khi khoá join là anonymous type ghép 2 cột — nó ném
                            //    "The LINQ expression could not be translated" ngay lúc chạy. Dạng truy
                            //    vấn con dưới đây dịch ra scalar subquery bình thường và giữ được thứ tự.
                            _context.TaskerServicePrices
                                .Where(p => p.TaskerId == ts.TaskerId && p.ServiceId == ts.ServiceId
                                            && p.EffectiveFrom <= now
                                            && (p.EffectiveTo == null || p.EffectiveTo > now))
                                .OrderByDescending(p => p.EffectiveFrom)
                                .Select(p => p.Price)
                                .FirstOrDefault(),
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
