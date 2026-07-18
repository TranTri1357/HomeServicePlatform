using FluentValidation;

namespace HomeServicePlatform.Application.Modules.Booking.Commands.CreateEmergencyBooking
{
    public class CreateEmergencyBookingCommandValidator : AbstractValidator<CreateEmergencyBookingCommand>
    {
        public CreateEmergencyBookingCommandValidator()
        {
            RuleFor(x => x.CustomerId)
                .GreaterThan(0).WithMessage("Mã khách hàng không hợp lệ.");

            RuleFor(x => x.ServiceId)
                .GreaterThan(0).WithMessage("Mã dịch vụ không hợp lệ.");

            // Toạ độ dùng để quét thợ trong bán kính — sai toạ độ thì broadcast trượt hết.
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90d, 90d).WithMessage("Vĩ độ không hợp lệ.");

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180d, 180d).WithMessage("Kinh độ không hợp lệ.");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên người nhận không được để trống.")
                .MaximumLength(100).WithMessage("Tên người nhận không được vượt quá 100 ký tự.");

            // Cùng định dạng với CreateBookingValidator để hai luồng đặt đơn nhất quán.
            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Số điện thoại không được để trống.")
                .Matches(@"^(03|05|07|08|09)\d{8}$").WithMessage("Số điện thoại không đúng định dạng di động Việt Nam (phải gồm 10 chữ số).");

            RuleFor(x => x.AddressLine)
                .NotEmpty().WithMessage("Địa chỉ chi tiết không được để trống.")
                .MaximumLength(255).WithMessage("Địa chỉ chi tiết không được vượt quá 255 ký tự.");

            RuleFor(x => x.Note)
                .MaximumLength(500).WithMessage("Ghi chú không được vượt quá 500 ký tự.")
                .When(x => x.Note != null);
        }
    }
}
