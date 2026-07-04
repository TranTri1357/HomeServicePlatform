using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Commissions.Commands.CreateCommission
{
    public class CreateCommissionCommandHandler : IRequestHandler<CreateCommissionCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;
        public CreateCommissionCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<long>> Handle(CreateCommissionCommand request, CancellationToken ct)
        {
            if (request.CommissionRate < 0 || request.CommissionRate > 100)
            {
                throw new BadRequestException("Tỷ lệ chiết khấu hoa hồng phải nằm trong khoảng từ 0% đến 100%.");
            }

            var now = DateTimeOffset.UtcNow;

            long? targetServiceId = request.ServiceId <= 0 ? null : request.ServiceId;
            long? targetTaskerId = request.TaskerId <= 0 ? null : request.TaskerId;

            DateTimeOffset finalEffectiveFrom = request.EffectiveFrom.ToUniversalTime();
            DateTimeOffset? finalEffectiveTo = request.EffectiveTo?.ToUniversalTime();

            if (finalEffectiveTo.HasValue && finalEffectiveFrom >= finalEffectiveTo.Value)
            {
                throw new BadRequestException("Thời gian kết thúc hiệu lực (EffectiveTo) phải lớn hơn thời gian bắt đầu (EffectiveFrom).");
            }

           
            var duplicateActiveCommission = await _context.Commissions
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ServiceId == targetServiceId
                                       && c.TaskerId == targetTaskerId
                                       && (c.EffectiveTo == null || c.EffectiveTo > now), ct);

            if (duplicateActiveCommission != null)
            {
                var duplicateEntity = new Commission { CommissionId = duplicateActiveCommission.CommissionId };
                _context.Commissions.Attach(duplicateEntity);
                duplicateEntity.EffectiveTo = now;
                if (_context is DbContext efContext)
                {
                    efContext.Entry(duplicateEntity).Property(x => x.EffectiveTo).IsModified = true;
                }
            }

            
            var commission = new Commission
            {
                ServiceId = targetServiceId,
                TaskerId = targetTaskerId,
                CommissionRate = request.CommissionRate,
                EffectiveFrom = finalEffectiveFrom,
                EffectiveTo = finalEffectiveTo,
                CreatedAt = now
            };

            _context.Commissions.Add(commission);

            
            await _context.SaveChangesAsync(ct);

            return ApiResponse<long>.Success(commission.CommissionId, "Thêm mới và tối ưu cấu hình hoa hồng thành công.");
        }
    }
}
