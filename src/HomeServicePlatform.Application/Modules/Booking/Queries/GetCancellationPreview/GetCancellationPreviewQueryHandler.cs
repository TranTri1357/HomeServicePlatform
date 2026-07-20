using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;

namespace HomeServicePlatform.Application.Modules.Booking.Queries.GetCancellationPreview
{
    public class GetCancellationPreviewQueryHandler
        : IRequestHandler<GetCancellationPreviewQuery, ApiResponse<CancellationPreviewDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly RefundPolicyOptions _policy;
        private readonly BookingPolicyOptions _bookingPolicy;

        public GetCancellationPreviewQueryHandler(
            IApplicationDbContext context,
            IOptions<RefundPolicyOptions> policy,
            IOptions<BookingPolicyOptions> bookingPolicy)
        {
            _context = context;
            _policy = policy.Value;
            _bookingPolicy = bookingPolicy.Value;
        }

        public async Task<ApiResponse<CancellationPreviewDto>> Handle(GetCancellationPreviewQuery request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.BookingItems)
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);

            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch số #{request.BookingId}");

            if (booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền xem đơn này.");

            var totalPaid = await _context.Payments
                .Where(p => p.BookingId == booking.BookingId && p.Status == (short)PaymentStatus.Paid)
                .SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;

            var scheduledAt = booking.BookingItems.Count > 0
                ? booking.BookingItems.Min(bi => bi.StartAt)
                : (DateTimeOffset?)null;

            var decision = RefundPolicy.Calculate(
                booking.Status, scheduledAt, DateTimeOffset.UtcNow, RefundInitiator.Customer,
                totalPaid, booking.FinalAmount, _bookingPolicy.DepositPercent, _policy);

            var dto = new CancellationPreviewDto(
                booking.BookingId,
                decision.CanCancel,
                (short)booking.Status,
                totalPaid,
                decision.RefundPercent,
                decision.RefundAmount,
                decision.PenaltyAmount,
                decision.DepositAtRisk,
                decision.Reason);

            return ApiResponse<CancellationPreviewDto>.Success(dto, "Xem trước chính sách hủy đơn.");
        }
    }
}
