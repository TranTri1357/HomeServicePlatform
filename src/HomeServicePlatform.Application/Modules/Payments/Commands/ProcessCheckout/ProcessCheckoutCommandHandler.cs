using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeServicePlatform.Application.Common.Exceptions;
using HomeServicePlatform.Application.Common.Helpers;
using HomeServicePlatform.Application.Common.Interfaces;
using HomeServicePlatform.Application.Common.Options;
using HomeServicePlatform.Application.Common.Responses;
using Microsoft.Extensions.Options;
using HomeServicePlatform.Domain.Modules.Payments.Entities;
using HomeServicePlatform.Domain.Modules.Payments.Enum;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommandHandler : IRequestHandler<ProcessCheckoutCommand, ApiResponse<CheckoutResponse>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IEnumerable<IPaymentStrategy> _strategies;
        private readonly BookingPolicyOptions _bookingPolicy;

        public ProcessCheckoutCommandHandler(
            IApplicationDbContext context,
            IEnumerable<IPaymentStrategy> strategies,
            IOptions<BookingPolicyOptions> bookingPolicy)
        {
            _context = context;
            _strategies = strategies;
            _bookingPolicy = bookingPolicy.Value;
        }

        public async Task<ApiResponse<CheckoutResponse>> Handle(ProcessCheckoutCommand request, CancellationToken ct)
        {
            var booking = await _context.Bookings
                .FirstOrDefaultAsync(b => b.BookingId == request.BookingId, ct);
            if (booking == null)
                throw new NotFoundException($"Không tìm thấy đơn đặt lịch #{request.BookingId}.");

            if (booking.CustomerId != request.CustomerId)
                throw new ForbiddenException("Bạn không có quyền thanh toán đơn hàng của người khác.");

            if (booking.Status != Domain.Modules.Bookings.Enums.BookingStatus.Pending)
                throw new BadRequestException("Đơn hàng không ở trạng thái chờ thanh toán.");

            var alreadyPaid = await _context.Payments
                .AnyAsync(p => p.BookingId == request.BookingId && p.Status == (short)PaymentStatus.Paid, ct);
            if (alreadyPaid)
                throw new BadRequestException("Đơn hàng này đã được thanh toán trước đó.");

            decimal amount = request.IsDeposit
                ? RefundPolicy.DepositOf(booking.FinalAmount, _bookingPolicy.DepositPercent)
                : booking.FinalAmount;
            if (amount <= 0)
                throw new BadRequestException("Số tiền cần thanh toán của đơn không hợp lệ.");

            var strategy = _strategies.FirstOrDefault(s => s.Method == request.Method);
            if (strategy == null)
                throw new BadRequestException("Phương thức thanh toán này hiện chưa được hệ thống hỗ trợ tích hợp.");

            var now = DateTimeOffset.UtcNow;

            var strategyResult = await strategy.ProcessPaymentAsync(request.BookingId, amount, ct);

            var payment = new Payment
            {
                BookingId = request.BookingId,
                Amount = amount,
                Method = (short)request.Method,
                Status = (short)(strategyResult.IsInstantSuccess ? PaymentStatus.Paid : PaymentStatus.Pending),
                TransactionCode = strategyResult.TransactionCode,
                PaidAt = strategyResult.IsInstantSuccess ? now : null,
                CreatedAt = now,
                RowVersion = 1
            };

            _context.Payments.Add(payment);

            if (strategyResult.IsInstantSuccess || (short)request.Method == 2)
            {
                var taskerIds = await _context.BookingItems
                    .Where(bi => bi.BookingId == request.BookingId && bi.TaskerId != null)
                    .Select(bi => bi.TaskerId!.Value)
                    .Distinct()
                    .ToListAsync(ct);

                foreach (var taskerId in taskerIds)
                {
                    _context.Notifications.Add(Common.Helpers.NotificationBuilder.Build(
                        taskerId,
                        Domain.Modules.Operations.Enum.NotificationType.NewBooking,
                        "Bạn có đơn mới",
                        $"Bạn có đơn đặt lịch mới (BK{request.BookingId}) đã thanh toán. Hãy vào xác nhận."));
                }
            }


            await _context.SaveChangesAsync(ct);

            var checkoutResult = new CheckoutResponse(
                payment.PaymentId,
                strategyResult.IsInstantSuccess,
                strategyResult.PaymentUrl
            );

            return ApiResponse<CheckoutResponse>.Success(checkoutResult, "Khởi tạo luồng kết toán hóa đơn thành công.");
        }
    }
}
