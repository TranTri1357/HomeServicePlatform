using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.RaiseDispute
{
    public class RaiseDisputeCommandHandler : IRequestHandler<RaiseDisputeCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;
        public RaiseDisputeCommandHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<long>> Handle(RaiseDisputeCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings.AnyAsync(b => b.BookingId == request.BookingId, ct);
            if (!booking) throw new NotFoundException($"Không tìm thấy đơn đặt lịch #{request.BookingId}");

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new BadRequestException("Lý do khiếu nại tranh chấp không được để trống.");

            var dispute = new Dispute
            {
                BookingId = request.BookingId,
                RaisedById = request.RaisedById,
                Reason = request.Reason.Trim(),
                Status = 0,
                ResolutionNote = null,
                RefundAmount = null,
                ResolvedAt = null,
                CreatedAt = DateTimeOffset.UtcNow,
                RowVersion = 1
            };

            _context.Disputes.Add(dispute);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<long>.Success(dispute.DisputeId, "Gửi khiếu nại tranh chấp lên hệ thống thành công.");
        }
    }
}
