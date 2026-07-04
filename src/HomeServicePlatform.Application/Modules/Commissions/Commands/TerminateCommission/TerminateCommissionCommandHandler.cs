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

namespace HomeServicePlatform.Application.Modules.Commissions.Commands.TerminateCommission
{
    public class TerminateCommissionCommandHandler : IRequestHandler<TerminateCommissionCommand, ApiResponse<bool>>
    {
        private readonly IApplicationDbContext _context;
        public TerminateCommissionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<bool>> Handle(TerminateCommissionCommand request, CancellationToken ct)
        {
            var now = DateTimeOffset.UtcNow;

            var commission = await _context.Commissions
                .FirstOrDefaultAsync(c => c.CommissionId == request.CommissionId, ct);

            if (commission == null)
                throw new NotFoundException($"Không tìm thấy cấu hình hoa hồng số #{request.CommissionId}");

            if (commission.EffectiveTo.HasValue && commission.EffectiveTo.Value <= now)
            {
                throw new BadRequestException($"Biểu phí hoa hồng này đã được đóng hiệu lực từ trước (vào lúc: {commission.EffectiveTo.Value:dd/MM/yyyy HH:mm:ss} UTC).");
            }
            commission.EffectiveTo = now;; 

            await _context.SaveChangesAsync(ct);
            return ApiResponse<bool>.Success(true, "Đã đóng hiệu lực tỉ lệ hoa hồng thành công.");
        }
    }
}
