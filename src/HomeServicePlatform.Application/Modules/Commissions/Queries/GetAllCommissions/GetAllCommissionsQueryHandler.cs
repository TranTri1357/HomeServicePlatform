using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Pagination;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission;

namespace HomeServicePlatform.Application.Modules.Commissions.Queries.GetAllCommissions
{
    public class UpdateCommissionCommandHandler : IRequestHandler<UpdateCommissionCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public UpdateCommissionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(UpdateCommissionCommand request, CancellationToken ct)
        {
            // 1. Kiểm tra tỷ lệ phần trăm hợp lệ
            if (request.CommissionRate < 0 || request.CommissionRate > 100)
            {
                throw new BadRequestException("Tỷ lệ chiết khấu hoa hồng phải nằm trong khoảng từ 0% đến 100%.");
            }

            // 🟢 ĐỒNG BỘ MÚI GIỜ CHUẨN: Sử dụng định dạng UTC đồng nhất toàn hệ thống
            var now = DateTimeOffset.UtcNow;

            // 2. Tìm chính xác bản ghi cần sửa đổi qua ID lấy từ URL Route
            var commissionToUpdate = await _context.Commissions
                .FirstOrDefaultAsync(c => c.CommissionId == request.CommissionId, ct);

            if (commissionToUpdate == null)
            {
                throw new NotFoundException($"Không tìm thấy cấu hình hoa hồng mang mã số #{request.CommissionId}");
            }

            // Quy đổi giá trị an toàn cho bộ lọc
            long? targetServiceId = request.ServiceId <= 0 ? null : request.ServiceId;
            long? targetTaskerId = request.TaskerId <= 0 ? null : request.TaskerId;

            // 3. 🛡️ KIỂM TRA TRÙNG LẶP: Tìm xem có dòng nào KHÁC đang chiếm giữ cặp Service/Thợ này không
            var duplicateActiveCommission = await _context.Commissions
                .FirstOrDefaultAsync(c => c.CommissionId != request.CommissionId
                                       && c.ServiceId == targetServiceId
                                       && c.TaskerId == targetTaskerId
                                       && (c.EffectiveTo == null || c.EffectiveTo > now), ct);

            if (duplicateActiveCommission != null)
            {
                // Nếu trùng khớp hoàn toàn, ta đóng bản ghi trùng đó lại để nhường chỗ cho bản ghi hiện tại
                duplicateActiveCommission.EffectiveTo = now;
            }

            // 4. BẪY LỖI NGÀY THÁNG (Bảo vệ Check Constraint dưới DB)
            DateTimeOffset? finalEffectiveTo = request.EffectiveTo?.ToUniversalTime();

            // 🟢 GIẢI PHÁP SỬA LỖI CHÍ MẠNG: 
            // Nếu sửa EffectiveFrom thành 'now', phải chắc chắn nó nhỏ hơn ngày kết thúc do FE gửi lên
            if (finalEffectiveTo.HasValue && now >= finalEffectiveTo.Value)
            {
                throw new BadRequestException("Thời gian kết thúc hiệu lực biểu phí phải lớn hơn thời gian hiện tại.");
            }

            // 5. TIẾN HÀNH CẬP NHẬT TRỰC TIẾP
            commissionToUpdate.ServiceId = targetServiceId;
            commissionToUpdate.TaskerId = targetTaskerId;
            commissionToUpdate.CommissionRate = request.CommissionRate;

            // Thiết lập đồng bộ thời gian hiệu lực
            commissionToUpdate.EffectiveFrom = now;
            commissionToUpdate.EffectiveTo = finalEffectiveTo;

            // 6. Lưu xuống DB an toàn
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật cấu hình hoa hồng trực tiếp thành công.");
        }
    }
}
