using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CancelBookingByCustomer
{
    public class CancelBookingByCustomerValidator : AbstractValidator<CancelBookingByCustomerCommand>
    {
        public CancelBookingByCustomerValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("Mã đơn đặt lịch không hợp lệ.");

            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Mã khách hàng không hợp lệ.");

            RuleFor(x => x.CancelReason)
                .NotEmpty().WithMessage("Vui lòng nhập lý do hủy đơn.")
                .MaximumLength(500).WithMessage("Lý do hủy không được vượt quá 500 ký tự.");
        }
    }
}
