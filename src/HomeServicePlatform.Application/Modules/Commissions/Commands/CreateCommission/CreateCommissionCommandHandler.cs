using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
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

            // 🏦 Tìm các biểu phí CHỒNG LẤN khoảng hiệu lực với biểu phí sắp tạo.
            //    Trước đây chỗ này hỏi "có biểu phí nào đang chạy LÚC NÀY không" rồi đóng nó
            //    tại thời điểm `now`. Sai ở hai điểm:
            //      1) Biểu phí hẹn trước cho tương lai lọt lưới (chưa chạy lúc này nhưng vẫn
            //         chồng lấn với khoảng sắp tạo) → hai biểu phí cùng áp một lúc.
            //      2) Nếu biểu phí mới bắt đầu ở TƯƠNG LAI mà lại đóng biểu phí cũ NGAY BÂY GIỜ
            //         thì sinh ra KHOẢNG TRỐNG không biểu phí nào hiệu lực; trong khoảng đó
            //         CommissionResolver trả 0% → sàn mất trắng hoa hồng cho tới ngày biểu phí
            //         mới có hiệu lực.
            //    Nay: hỏi theo CHỒNG LẤN, và đóng biểu phí cũ ĐÚNG LÚC biểu phí mới bắt đầu
            //    → nối liền mạch, không hở, không đè.
            var overlapping = await _context.Commissions
                .AsNoTracking()
                .Where(c => c.ServiceId == targetServiceId && c.TaskerId == targetTaskerId)
                .Where(CommissionQuery.OverlapsWith(finalEffectiveFrom, finalEffectiveTo))
                .ToListAsync(ct);

            foreach (var old in overlapping)
            {
                // Biểu phí cũ bắt đầu SAU thời điểm biểu phí mới có hiệu lực thì không thể
                // "cắt đuôi" cho gọn được — báo lỗi để admin tự xử lý thay vì âm thầm ghi đè.
                if (old.EffectiveFrom >= finalEffectiveFrom)
                {
                    throw new BadRequestException(
                        $"Đã tồn tại biểu phí #{old.CommissionId} ({old.CommissionRate}%) có hiệu lực từ " +
                        $"{old.EffectiveFrom:dd/MM/yyyy HH:mm} UTC, chồng lấn với khoảng bạn vừa nhập. " +
                        "Vui lòng đóng hiệu lực biểu phí đó trước.");
                }

                var oldEntity = new Commission { CommissionId = old.CommissionId };
                _context.Commissions.Attach(oldEntity);
                oldEntity.EffectiveTo = finalEffectiveFrom; // nối liền mạch, không tạo khoảng trống
                if (_context is DbContext efContext)
                {
                    efContext.Entry(oldEntity).Property(x => x.EffectiveTo).IsModified = true;
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
