using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using MediatR;
using HomeServicePlatform.Domain.Modules.Operations.Entities;

namespace HomeServicePlatform.Application.Modules.Commissions.Commands.UpdateCommission
{
    public class UpdateCommissionCommandHandler : IRequestHandler<UpdateCommissionCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public UpdateCommissionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(UpdateCommissionCommand request, CancellationToken ct)
        {
            // 1. Ràng buộc dữ liệu cơ bản
            if (request.CommissionRate < 0 || request.CommissionRate > 100)
            {
                throw new BadRequestException("Tỷ lệ chiết khấu hoa hồng phải nằm trong khoảng từ 0% đến 100%.");
            }

            var now = DateTimeOffset.UtcNow;

            // 2. Tìm chính xác bản ghi cần Update qua ID
            var commissionToUpdate = await _context.Commissions
                .FirstOrDefaultAsync(c => c.CommissionId == request.CommissionId, ct);

            if (commissionToUpdate == null)
            {
                throw new NotFoundException($"Không tìm thấy cấu hình hoa hồng mang mã số #{request.CommissionId}");
            }

            // Chuẩn hóa dữ liệu đầu vào (0 hoặc âm quy về null - áp dụng toàn sàn/tất cả thợ)
            long? targetServiceId = request.ServiceId <= 0 ? null : request.ServiceId;
            long? targetTaskerId = request.TaskerId <= 0 ? null : request.TaskerId;

            // 3. 🛡️ LOGIC CHỐNG CHỒNG CHẤT HOA HỒNG (Anti-Overlap Validation)
            // Tìm xem trong hệ thống CÓ bản ghi NÀO KHÁC (khác ID đang sửa) cũng đang active cho cặp Service/Tasker này không
            var duplicateActiveCommission = await _context.Commissions
                .FirstOrDefaultAsync(c => c.CommissionId != request.CommissionId
                                       && c.ServiceId == targetServiceId
                                       && c.TaskerId == targetTaskerId
                                       && (c.EffectiveTo == null || c.EffectiveTo > now), ct);

            if (duplicateActiveCommission != null)
            {
                // Nếu trùng mức %, không cần tạo thêm xung đột, đóng bản ghi cũ lại
                if (duplicateActiveCommission.CommissionRate == request.CommissionRate)
                {
                    duplicateActiveCommission.EffectiveTo = now;
                }
                else
                {
                    // Nếu khác %, chặn đứng và thông báo cho Admin xử lý đóng bản ghi trùng trước
                    throw new BadRequestException($"Đã tồn tại một cấu hình hoa hồng khác ({duplicateActiveCommission.CommissionRate}%) đang chạy cho dịch vụ/thợ này. Vui lòng đóng hiệu lực của cấu hình đó trước.");
                }
            }

            // 4. 🟢 THỰC HIỆN CẬP NHẬT TRỰC TIẾP (Không sinh lệnh INSERT mới)
            commissionToUpdate.ServiceId = targetServiceId;
            commissionToUpdate.TaskerId = targetTaskerId;
            commissionToUpdate.CommissionRate = request.CommissionRate;

            // Thiết lập lại thời gian hiệu lực theo dữ liệu mới
            commissionToUpdate.EffectiveFrom = now;
            commissionToUpdate.EffectiveTo = null; // Reset về vô hạn hiệu lực (đang active)

            if (request.EffectiveTo.HasValue)
            {
                commissionToUpdate.EffectiveTo = request.EffectiveTo.Value.ToUniversalTime();
            }

            // 5. Lưu thay đổi xuống Database
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật cấu hình hoa hồng thành công.");
        }
    }
}
