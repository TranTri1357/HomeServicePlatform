using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Operations.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Disputes.Commands.CreateDispute
{
    public class CreateDisputeCommandHandler : IRequestHandler<CreateDisputeCommand, ApiResponse<long>>
    {
        private readonly IApplicationDbContext _context;

        public CreateDisputeCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<long>> Handle(CreateDisputeCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException("Không tìm thấy đơn hàng này.");

            if (booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền thực hiện thao tác trên đơn hàng này.");

            var existingDispute = await _context.Disputes
                .AnyAsync(d => d.BookingId == request.BookingId && d.Status == 0, ct);

            if (existingDispute)
                throw new BadRequestException("Đơn hàng này đã có khiếu nại đang chờ xử lý. Vui lòng đợi Admin phản hồi.");

            var dispute = new Dispute
            {
                BookingId = request.BookingId,
                RaisedById = request.CustomerId,
                Reason = request.Reason.Trim(),
                Status = 0
            };

            _context.Disputes.Add(dispute);
            await _context.SaveChangesAsync(ct);

            return ApiResponse<long>.Success(dispute.DisputeId, "Gửi khiếu nại thành công. Đội ngũ CSKH sẽ liên hệ với bạn sớm nhất.");
        }
    }
}
