using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.DeclinePendingBooking
{
    public class DeclinePendingBookingValidator : AbstractValidator<DeclinePendingBookingCommand>
    {
        public DeclinePendingBookingValidator()
        {
            RuleFor(x => x.BookingId)
                .GreaterThan(0).WithMessage("Mã đơn đặt lịch không hợp lệ.");

            RuleFor(x => x.TaskerId)
                .GreaterThan(0).WithMessage("Mã thợ không hợp lệ.");

            RuleFor(x => x.DeclineReason)
                .NotEmpty().WithMessage("Vui lòng nhập lý do từ chối đơn.")
                .MaximumLength(500).WithMessage("Lý do từ chối không được vượt quá 500 ký tự.");
        }
    }
}
