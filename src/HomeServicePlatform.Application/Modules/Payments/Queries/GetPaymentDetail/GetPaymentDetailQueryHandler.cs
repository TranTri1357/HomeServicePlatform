using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Application.Modules.Payments.Queries.GetPagedPayments;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Payments.Queries.GetPaymentDetail
{
    public class GetPaymentDetailQueryHandler : IRequestHandler<GetPaymentDetailQuery, ApiResponse<PaymentLookupDto>>
    {
        private readonly IApplicationDbContext _context;
        public GetPaymentDetailQueryHandler(IApplicationDbContext context) => _context = context;

        public async Task<ApiResponse<PaymentLookupDto>> Handle(GetPaymentDetailQuery request, CancellationToken ct)
        {
            var data = await _context.Payments
                .AsNoTracking()
                .Where(x => x.PaymentId == request.PaymentId)
                .Select(x => new PaymentLookupDto(
                    x.PaymentId,
                    x.BookingId,
                    x.Amount,
                    x.Method,
                    x.Status,
                    x.TransactionCode,
                    x.PaidAt,
                    x.CreatedAt
                )).FirstOrDefaultAsync(ct);

            if (data == null) throw new NotFoundException($"Không tìm thấy giao dịch thanh toán #{request.PaymentId}");

            return ApiResponse<PaymentLookupDto>.Success(data, "Lấy chi tiết thanh toán thành công.");
        }
    }
}
