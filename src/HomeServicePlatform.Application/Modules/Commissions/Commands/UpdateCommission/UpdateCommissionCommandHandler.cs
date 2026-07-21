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
            if (request.CommissionRate < 0 || request.CommissionRate > 100)
            {
                throw new BadRequestException("Tỷ lệ chiết khấu hoa hồng phải nằm trong khoảng từ 0% đến 100%.");
            }

            var now = DateTimeOffset.UtcNow;

            var commissionToUpdate = await _context.Commissions
                .FirstOrDefaultAsync(c => c.CommissionId == request.CommissionId, ct);

            if (commissionToUpdate == null)
            {
                throw new NotFoundException($"Không tìm thấy cấu hình hoa hồng mang mã số #{request.CommissionId}");
            }

            long? targetServiceId = request.ServiceId <= 0 ? null : request.ServiceId;
            long? targetTaskerId = request.TaskerId <= 0 ? null : request.TaskerId;

            var newEffectiveTo = request.EffectiveTo?.ToUniversalTime();
            if (newEffectiveTo.HasValue && newEffectiveTo.Value <= now)
            {
                throw new BadRequestException(
                    "Thời gian kết thúc hiệu lực (EffectiveTo) phải nằm sau thời điểm hiện tại.");
            }

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

            if (commissionToUpdate.EffectiveFrom >= now)
            {
                commissionToUpdate.CommissionRate = request.CommissionRate;
                commissionToUpdate.EffectiveTo = newEffectiveTo;
            }
            else
            {
                commissionToUpdate.EffectiveTo = now;

                _context.Commissions.Add(new Commission
                {
                    ServiceId = targetServiceId,
                    TaskerId = targetTaskerId,
                    CommissionRate = request.CommissionRate,
                    EffectiveFrom = now,
                    EffectiveTo = newEffectiveTo,
                    CreatedAt = now
                });
            }

            await _context.SaveChangesAsync(ct);

            return ApiResponse<bool>.Success(true, "Cập nhật cấu hình hoa hồng thành công.");
        }
    }
}
