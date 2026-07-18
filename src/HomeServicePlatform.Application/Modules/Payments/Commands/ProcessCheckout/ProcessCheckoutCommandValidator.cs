using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Payments.Commands.ProcessCheckout
{
    public class ProcessCheckoutCommandValidator : AbstractValidator<ProcessCheckoutCommand>
    {
        public ProcessCheckoutCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Mã khách hàng không hợp lệ.");

            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("Mã đơn hàng không hợp lệ.");

            // ⚠️ Không validate số tiền: server tự tính từ Booking.FinalAmount (chống giả mạo giá).
            RuleFor(x => x.Method)
                .IsInEnum().WithMessage("Phương thức thanh toán không hợp lệ.");
        }
    }
}
