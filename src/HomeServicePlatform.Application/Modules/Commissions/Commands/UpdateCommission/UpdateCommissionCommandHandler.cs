using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
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

            // Khoảng hiệu lực SAU khi cập nhật (bước 4 bên dưới ép EffectiveFrom = now).
            var newEffectiveTo = request.EffectiveTo?.ToUniversalTime();
            if (newEffectiveTo.HasValue && newEffectiveTo.Value <= now)
            {
                throw new BadRequestException(
                    "Thời gian kết thúc hiệu lực (EffectiveTo) phải nằm sau thời điểm hiện tại.");
            }

            // 3. Không cho đổi ĐỐI TƯỢNG áp dụng của một biểu phí đang tồn tại.
            //    Đổi từ "dịch vụ A" sang "dịch vụ B" không phải là sửa — đó là một quy tắc KHÁC.
            //    Cho phép đổi sẽ làm bốc hơi lịch sử biểu phí của dịch vụ A.
            if (targetServiceId != commissionToUpdate.ServiceId || targetTaskerId != commissionToUpdate.TaskerId)
            {
                throw new BadRequestException(
                    "Không thể đổi dịch vụ/thợ áp dụng của một biểu phí đã tạo. " +
                    "Hãy đóng hiệu lực biểu phí này rồi tạo biểu phí mới cho đối tượng khác.");
            }

            if (commissionToUpdate.EffectiveTo.HasValue && commissionToUpdate.EffectiveTo.Value <= now)
            {
                throw new BadRequestException(
                    "Biểu phí này đã hết hiệu lực nên không sửa được. Vui lòng tạo biểu phí mới.");
            }

            // 4. 🛡️ CHỐNG CHỒNG CHẤT HOA HỒNG (Anti-Overlap Validation)
            // 🏦 Hỏi theo CHỒNG LẤN khoảng thời gian (xem CommissionQuery.OverlapsWith), không phải
            //    "có đang chạy lúc này không" — hỏi kiểu cũ thì biểu phí hẹn trước cho tương lai
            //    lọt lưới và hệ thống sẽ có hai biểu phí cùng áp cho một cặp (dịch vụ, thợ).
            var duplicateActiveCommission = await _context.Commissions
                .Where(c => c.CommissionId != request.CommissionId
                            && c.ServiceId == targetServiceId
                            && c.TaskerId == targetTaskerId)
                .Where(CommissionQuery.OverlapsWith(now, newEffectiveTo))
                .FirstOrDefaultAsync(ct);

            if (duplicateActiveCommission != null)
            {
                throw new BadRequestException(
                    $"Đã tồn tại một cấu hình hoa hồng khác ({duplicateActiveCommission.CommissionRate}%) " +
                    "chồng lấn thời gian hiệu lực cho dịch vụ/thợ này. Vui lòng đóng hiệu lực của cấu hình đó trước.");
            }

            // 5. 🏦 GHI PHIÊN BẢN MỚI thay vì sửa đè (giữ lịch sử biểu phí).
            //
            //    Trước đây bước này sửa thẳng CommissionRate và ép EffectiveFrom = now trên chính
            //    dòng cũ. Hậu quả: một biểu phí 10% đã áp suốt 3 tháng, admin đổi sang 20% là dòng
            //    đó biến thành "20% từ hôm nay" — sự thật "10% đã áp 3 tháng" BỐC HƠI, không còn
            //    dấu vết nào để đối chiếu về sau.
            //
            //    Nay: đóng hiệu lực dòng cũ tại `now` rồi chèn dòng mới có hiệu lực từ `now`.
            //    Bảng Commission tự nó trở thành LỊCH SỬ biểu phí — không cần thêm cột audit,
            //    không cần migration. `CompleteWork` vốn đã phân giải theo thời điểm hoàn thành
            //    nên các đơn cũ vẫn giữ đúng tỷ lệ đã áp cho chúng.
            if (commissionToUpdate.EffectiveFrom >= now)
            {
                // Biểu phí chưa từng có hiệu lực (vừa tạo / hẹn trước) — sửa đè là an toàn,
                // không xoá mất lịch sử nào và tránh đẻ ra dòng rác dài 0 giây.
                commissionToUpdate.CommissionRate = request.CommissionRate;
                commissionToUpdate.EffectiveTo = newEffectiveTo;
            }
            else
            {
                commissionToUpdate.EffectiveTo = now; // chốt sổ phiên bản đang chạy

                _context.Commissions.Add(new Commission
                {
                    ServiceId = targetServiceId,
                    TaskerId = targetTaskerId,
                    CommissionRate = request.CommissionRate,
                    EffectiveFrom = now,
                    EffectiveTo = newEffectiveTo, // null = vô thời hạn
                    CreatedAt = now
                });
            }

            // 6. Lưu thay đổi xuống Database
            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật cấu hình hoa hồng thành công.");
        }
    }
}
